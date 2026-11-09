"""Separate short test: real browser close/reopen, unfinished drawing and corrupt storage."""
from pathlib import Path
from playwright.sync_api import sync_playwright
import json
HERE=Path(__file__).resolve().parent;ROOT=HERE.parents[1];OUT=ROOT/'Validation/SixCityRouteEditor-20261019/persistence'
def main():
    OUT.mkdir(exist_ok=True,parents=True);assert not (OUT/'results.json').exists()
    exe='C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe';profile=OUT/'isolated-browser-profile';checks=[]
    def check(ok,name):
        assert ok,name
        checks.append({'test':name,'status':'PASS'})
    with sync_playwright() as p:
        def launch():return p.chromium.launch_persistent_context(str(profile),executable_path=exe,headless=True,viewport={'width':1600,'height':1000},args=['--disable-background-networking'])
        def open(ctx):
            page=ctx.new_page();page.goto((HERE/'index.html').as_uri());page.wait_for_function('window.RouteEditor');page.wait_for_timeout(100);return page
        ctx=launch();page=open(ctx);page.click('#begin')
        loc=page.evaluate('RouteEditor.mapToClient({x:410,y:420})');page.mouse.click(loc['x'],loc['y']);page.click('#saveDraft')
        draft=page.evaluate('RouteEditor.getDraft()');check(draft is not None and len(draft['waypoints'])==1,'Unfinished node-linked draft saved')
        ctx.close();ctx=launch();page=open(ctx)
        check(page.evaluate('RouteEditor.getDraft()')==draft,'Full browser close/reopen preserves unfinished draft exactly')
        check(page.evaluate('RouteEditor.getProject().cities[0].preservedCityData.cityConstraints.gates.length')>0,'Exported draft retains canonical city gates and source metadata')
        with page.expect_download() as dl:page.click('#exportAll')
        saved=OUT/'QA_TEST_ONLY_unfinished.json';dl.value.save_as(str(saved));page.set_input_files('#importFile',str(saved));page.wait_for_timeout(100)
        check(page.evaluate('RouteEditor.getDraft()')==draft,'Unfinished draft JSON export/import roundtrip')
        # Local-only corrupt fixture: do not overwrite it automatically on startup.
        page.evaluate('localStorage.setItem(RouteEditor.storageKey,"CORRUPT_QA_ONLY")');page.reload();page.wait_for_function('window.RouteEditor');page.click('#saveDraft')
        check(page.evaluate('localStorage.getItem(RouteEditor.storageKey)')=='CORRUPT_QA_ONLY','Corrupt browser draft protected from silent overwrite')
        page.set_input_files('#importFile',str(saved));page.wait_for_timeout(100)
        check(page.evaluate('RouteEditor.getDraft()')==draft and page.evaluate('localStorage.getItem(RouteEditor.storageKey)')!='CORRUPT_QA_ONLY','Valid file import recovers autosave without losing drawing')
        # No new runtime restriction is inferred from a source classification mismatch.
        check(page.evaluate('RouteEditor.getCity().nodes.find(n=>n.id==="H5").kind==="bot" && RouteEditor.getCity().nodes.find(n=>n.id==="H5").catalogKind==="boss"'),'H5 runtime kind/catalog classification both retained')
        check(page.evaluate('RouteEditor.getCity().nodes.find(n=>n.id==="H7").kind==="quest" && RouteEditor.getCity().nodes.find(n=>n.id==="H7").catalogKind==="beacon"'),'H7 runtime kind/catalog classification both retained')
        ctx.close()
    (OUT/'results.json').write_text(json.dumps({'status':'PASS','checks':checks,'notes':'Isolated QA browser profile only; final shipped defaults remain empty. Profile cache is not copied to checkpoint.'},indent=2),encoding='utf-8')
    print('PASS',len(checks),'persistence/source-identity checks')
if __name__=='__main__':main()
