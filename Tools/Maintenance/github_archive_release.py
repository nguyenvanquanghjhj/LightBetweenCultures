"""Publish/verify owner-authorized archives with GitHub REST. Credentials stay in memory."""
import argparse
import json
import os
from pathlib import Path
import subprocess
import urllib.request
import urllib.error
import urllib.parse
import hashlib
import time

REPO = 'nguyenvanquanghjhj/LightBetweenCultures'


def digest(path):
    with path.open('rb') as stream:
        return hashlib.file_digest(stream, 'sha256').hexdigest()


class SafeRedirect(urllib.request.HTTPRedirectHandler):
    def redirect_request(self, req, fp, code, msg, headers, newurl):
        redirected = super().redirect_request(req, fp, code, msg, headers, newurl)
        if redirected and urllib.parse.urlparse(req.full_url).hostname != urllib.parse.urlparse(newurl).hostname:
            redirected.remove_header('Authorization')
        return redirected


class UploadProgress:
    def __init__(self, stream, name):
        self.stream=stream;self.name=name;self.count=0;self.reported=time.monotonic()
    def read(self, size=-1):
        block=self.stream.read(size);self.count+=len(block)
        if time.monotonic()-self.reported>=20:
            print(json.dumps(dict(name=self.name,uploadedBytes=self.count)),flush=True)
            self.reported=time.monotonic()
        return block


def token():
    env = dict(os.environ, GIT_TERMINAL_PROMPT='0', GCM_INTERACTIVE='Never')
    result = subprocess.run(['git', 'credential', 'fill'], input='protocol=https\nhost=github.com\n\n', text=True, capture_output=True, env=env)
    fields = dict(line.split('=', 1) for line in result.stdout.splitlines() if '=' in line)
    if result.returncode or not fields.get('password'):
        raise SystemExit('No usable stored GitHub credential; no credential value was printed.')
    return fields['password']


def request(path, credential, payload=None, method=None):
    url = path if path.startswith('https://') else 'https://api.github.com' + path
    data = json.dumps(payload).encode() if payload is not None else None
    headers = {'Authorization': 'Bearer ' + credential, 'Accept': 'application/vnd.github+json', 'X-GitHub-Api-Version': '2022-11-28', 'User-Agent': 'LBC-owner-authorized-archive'}
    if data is not None:
        headers['Content-Type'] = 'application/json'
    with urllib.request.urlopen(urllib.request.Request(url, data=data, headers=headers, method=method), timeout=90) as response:
        return json.load(response)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('mode', choices=['auth', 'ensure-private', 'upload', 'download', 'publish', 'inspect'])
    parser.add_argument('--repository', default=REPO)
    parser.add_argument('--tag', default='lbc-final-archive-2026')
    parser.add_argument('--commit', default='main')
    parser.add_argument('--kind', choices=['historical-zips','private-metadata','source'], default='source')
    parser.add_argument('--directory', type=Path)
    parser.add_argument('--output', type=Path)
    parser.add_argument('--notes', type=Path)
    parser.add_argument('--names', nargs='+')
    args = parser.parse_args()
    credential = token()
    if args.mode in ['upload', 'download', 'publish', 'inspect']:
        repo = request('/repos/'+args.repository, credential)
        if args.repository.endswith('-archive') and not repo['private']:
            raise SystemExit('Historical repository must remain private')
        api='/repos/'+args.repository+'/releases'
        try:
            release=request(api+'/tags/'+args.tag,credential)
        except urllib.error.HTTPError as error:
            if error.code!=404 or args.mode!='upload':
                raise
            release=request(api,credential,dict(tag_name=args.tag,target_commitish=args.commit,name='Light Between Cultures — Final archive 2026',draft=True,prerelease=False,
                            body='Owner-authorized archival snapshot. Verification and immutable artifact checksums will be attached. Source: https://github.com/'+REPO+'/tree/'+args.tag))
        if args.mode=='upload':
            if args.names:
                paths=[args.directory/name for name in args.names]
            elif args.kind=='historical-zips':
                paths=sorted(p for p in args.directory.glob('LightBetweenCultures_*.zip') if 'Source_Final' not in p.name)
            elif args.kind=='private-metadata':
                paths=[args.directory/n for n in ['LightBetweenCultures_GitHistory_2026.bundle','HISTORICAL_ARCHIVE_MANIFEST.json','SHA256SUMS.txt']]
            else:
                paths=[args.directory/n for n in ['LightBetweenCultures_Source_Final_2026.zip','SOURCE_ARCHIVE_MANIFEST.json']]
            existing={a['name']:a for a in request(api+'/'+str(release['id'])+'/assets?per_page=100',credential)}
            for path in paths:
                size=path.stat().st_size
                if size>=2*1024**3:
                    raise SystemExit('Asset exceeds 2 GiB: '+path.name)
                expected='sha256:'+digest(path)
                if path.name in existing:
                    asset=existing[path.name]
                    if asset['size']!=size or asset.get('digest') not in [None,expected]:
                        raise SystemExit('Existing remote asset differs; protected: '+path.name)
                    print(json.dumps(dict(name=path.name,status='already-uploaded',bytes=size)),flush=True)
                    continue
                url=release['upload_url'].split('{')[0]+'?'+urllib.parse.urlencode(dict(name=path.name))
                headers={'Authorization':'Bearer '+credential,'User-Agent':'LBC-owner-authorized-archive','Accept':'application/vnd.github+json','Content-Type':'application/octet-stream','Content-Length':str(size)}
                print(json.dumps(dict(name=path.name,status='uploading',bytes=size)),flush=True)
                with path.open('rb') as stream:
                    req=urllib.request.Request(url,data=UploadProgress(stream,path.name),headers=headers,method='POST')
                    with urllib.request.urlopen(req,timeout=300) as response:
                        asset=json.load(response)
                if asset['size']!=size or asset.get('digest') not in [None,expected]:
                    raise SystemExit('Uploaded asset digest/size mismatch')
                print(json.dumps(dict(name=path.name,status='uploaded',bytes=size,url=asset['browser_download_url'],digest=asset.get('digest'))),flush=True)
        elif args.mode=='download':
            args.directory.mkdir(parents=True,exist_ok=True)
            rows=[]
            assets=request(api+'/'+str(release['id'])+'/assets?per_page=100',credential)
            opener=urllib.request.build_opener(SafeRedirect())
            for asset in assets:
                target=args.directory/asset['name']
                if target.exists():
                    raise SystemExit('Existing download protected: '+target.name)
                print(json.dumps(dict(name=asset['name'],status='downloading',bytes=asset['size'])),flush=True)
                url=asset['url']+'?nonce='+str(time.time_ns())
                req=urllib.request.Request(url,headers={'Authorization':'Bearer '+credential,'Accept':'application/octet-stream','User-Agent':'LBC-archive-restore-test'})
                with opener.open(req,timeout=300) as response, target.open('xb') as output:
                    while block:=response.read(1024*1024):
                        output.write(block)
                actual=digest(target)
                original=args.output/asset['name'] if args.output else None
                if target.stat().st_size!=asset['size'] or (asset.get('digest') and asset['digest']!='sha256:'+actual) or (original and digest(original)!=actual):
                    raise SystemExit('Downloaded content mismatch: '+target.name)
                rows.append(dict(name=target.name,bytes=target.stat().st_size,sha256=actual,status='PASS',url=asset['browser_download_url']))
                (args.directory/'DOWNLOAD_VERIFICATION.json').write_text(json.dumps(dict(repository=args.repository,release=release['html_url'],assets=rows),indent=2)+'\n',encoding='utf-8')
                print(json.dumps(dict(name=target.name,status='download-verified',sha256=actual)),flush=True)
        elif args.mode=='publish':
            body=args.notes.read_text(encoding='utf-8-sig') if args.notes else release['body']
            release=request(api+'/'+str(release['id']),credential,dict(draft=False,body=body),method='PATCH')
        print(json.dumps(dict(release=release['html_url'],draft=release['draft'],tag=release['tag_name'],assets=[dict(name=a['name'],bytes=a['size'],digest=a.get('digest'),url=a['browser_download_url']) for a in request(api+'/'+str(release['id'])+'/assets?per_page=100',credential)]),indent=2),flush=True)
        return
    user = request('/user', credential)
    if args.mode == 'ensure-private':
        archive = 'nguyenvanquanghjhj/LightBetweenCultures-archive'
        try:
            repo = request('/repos/' + archive, credential)
        except urllib.error.HTTPError as error:
            if error.code != 404:
                raise
            if user['login'] != 'nguyenvanquanghjhj':
                raise SystemExit('Unexpected authenticated owner')
            repo = request('/user/repos', credential, dict(name='LightBetweenCultures-archive', private=True, auto_init=True,
                           description='Private long-term historical archives for LightBetweenCultures; owner-authorized 2026-11-20'))
        if not repo['private']:
            raise SystemExit('Archive repository must be private; refusing public historical uploads')
        print(json.dumps(dict(repository=repo['html_url'], private=repo['private'], defaultBranch=repo['default_branch'])))
        return
    repo = request('/repos/' + REPO, credential)
    print(json.dumps(dict(login=user['login'], repository=repo['full_name'], push=repo.get('permissions', {}).get('push'), releases=len(request('/repos/' + REPO + '/releases', credential)))))


if __name__ == '__main__':
    main()
