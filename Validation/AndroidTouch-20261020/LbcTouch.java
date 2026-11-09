import android.os.SystemClock;
import android.view.InputDevice;
import android.view.InputEvent;
import android.view.MotionEvent;
import java.lang.reflect.Method;

// Shell-only device QA tool. Never packaged into the game or its assets.
public final class LbcTouch {
    static Object manager; static Method inject; static long down;
    static void send(int action, float[] xy) throws Exception {
        int n=xy.length/2;
        MotionEvent.PointerProperties[] props=new MotionEvent.PointerProperties[n];
        MotionEvent.PointerCoords[] coords=new MotionEvent.PointerCoords[n];
        for(int i=0;i<n;i++) {
            props[i]=new MotionEvent.PointerProperties();props[i].id=i;props[i].toolType=MotionEvent.TOOL_TYPE_FINGER;
            coords[i]=new MotionEvent.PointerCoords();coords[i].x=xy[2*i];coords[i].y=xy[2*i+1];coords[i].pressure=1;coords[i].size=.1f;
        }
        MotionEvent event=MotionEvent.obtain(down,SystemClock.uptimeMillis(),action,n,props,coords,0,0,1,1,0,0,InputDevice.SOURCE_TOUCHSCREEN,0);
        if(!((Boolean)inject.invoke(manager,event,2)))throw new RuntimeException("Input injection rejected");
        event.recycle();SystemClock.sleep(25);
    }
    public static void main(String[] a) throws Exception {
        Class<?> type;
        try { type=Class.forName("android.hardware.input.InputManagerGlobal"); }
        catch(ClassNotFoundException e) { type=Class.forName("android.hardware.input.InputManager"); }
        manager=type.getMethod("getInstance").invoke(null);
        inject=type.getMethod("injectInputEvent",InputEvent.class,int.class);
        float x=Float.parseFloat(a[1]),y=Float.parseFloat(a[2]);down=SystemClock.uptimeMillis();
        if(a[0].equals("pinch")) {
            send(MotionEvent.ACTION_DOWN,new float[]{x-60,y});
            send(MotionEvent.ACTION_POINTER_DOWN|(1<<8),new float[]{x-60,y,x+60,y});
            for(int i=1;i<=12;i++)send(MotionEvent.ACTION_MOVE,new float[]{x-60-i*5,y,x+60+i*5,y});
            send(MotionEvent.ACTION_POINTER_UP|(1<<8),new float[]{x-120,y,x+120,y});
            SystemClock.sleep(400);
            send(MotionEvent.ACTION_UP,new float[]{x-120,y});
        } else if(a[0].equals("cancel")) {
            send(MotionEvent.ACTION_DOWN,new float[]{x,y});SystemClock.sleep(100);
            send(MotionEvent.ACTION_CANCEL,new float[]{x,y});
        }
        System.out.println("INJECTED "+a[0]+" through Android InputManager; no game API calls");
    }
}
