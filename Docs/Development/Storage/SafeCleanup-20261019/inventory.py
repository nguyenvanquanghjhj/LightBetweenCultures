"""Metadata-only storage inventory. Never follows links and never deletes files."""
import os, json, heapq, shutil, sys, time
from pathlib import Path
ROOT = Path(r'D:\GAMEKKK\LightBetweenCultures')
OUT = ROOT / 'Docs/Storage/SafeCleanup-20261019'
def scan(root):
    directories, special, errors, links, largest = [], [], [], [], []
    def visit(path):
        size = count = 0
        try:
            with os.scandir(path) as entries:
                for e in entries:
                    try:
                        s = e.stat(follow_symlinks=False)
                        if e.is_symlink() or getattr(s, 'st_file_attributes', 0) & 1024:
                            links.append(e.path); continue
                        if e.is_dir(follow_symlinks=False):
                            b, n = visit(e.path); size += b; count += n
                        else:
                            size += s.st_size; count += 1
                            row = (s.st_size, e.path, s.st_mtime_ns)
                            if len(largest) < 1500: heapq.heappush(largest, row)
                            elif row > largest[0]: heapq.heapreplace(largest, row)
                            if Path(e.name).suffix.lower() in ('.apk', '.aab', '.keystore', '.jks'):
                                special.append({'path':e.path,'bytes':s.st_size,'mtime_ns':s.st_mtime_ns})
                    except OSError as ex: errors.append({'path': e.path, 'error': str(ex)})
        except OSError as ex: errors.append({'path':str(path), 'error':str(ex)})
        directories.append({'path':str(path),'bytes':size,'files':count})
        return size, count
    size, count = visit(str(root))
    return {'root':str(root),'bytes':size,'files':count,'directories':sorted(directories,key=lambda x:x['bytes'],reverse=True),
            'largest_files':[{'bytes':b,'path':p,'mtime_ns':m} for b,p,m in sorted(largest,reverse=True)],
            'special_files':special,'skipped_links':links,'errors':errors}
label = sys.argv[1] if len(sys.argv)>1 else 'before'
result = {'time':time.strftime('%Y-%m-%dT%H:%M:%S%z'), 'disk':dict(zip(('total','used','free'),shutil.disk_usage(ROOT))),
          'project':scan(ROOT), 'backups':scan(Path(r'D:\GAMEKKK\Backups'))}
(OUT / (label+'.json')).write_text(json.dumps(result,ensure_ascii=False,indent=2),encoding='utf-8')
for name in ('project','backups'):
    data=result[name]
    print(name, 'bytes',data['bytes'],'GiB',round(data['bytes']/2**30,3),'files',data['files'],'errors',len(data['errors']))
    direct=[d for d in data['directories'] if Path(d['path']).parent == Path(data['root'])]
    for d in direct[:18]: print(' ',round(d['bytes']/2**30,3),'GiB',d['path'])
print('Free disk GiB',round(result['disk']['free']/2**30,3))
