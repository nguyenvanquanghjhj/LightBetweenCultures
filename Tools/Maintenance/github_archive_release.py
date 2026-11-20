"""Publish/verify owner-authorized archives with GitHub REST. Credentials stay in memory."""
import argparse
import json
import os
from pathlib import Path
import subprocess
import urllib.request
import urllib.error

REPO = 'nguyenvanquanghjhj/LightBetweenCultures'


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
    parser.add_argument('mode', choices=['auth', 'ensure-private'])
    args = parser.parse_args()
    credential = token()
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
