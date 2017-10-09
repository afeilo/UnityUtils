package main;

import android.annotation.TargetApi;
import android.content.Context;
import android.database.ContentObserver;
import android.database.Cursor;
import android.graphics.BitmapFactory;
import android.graphics.Point;
import android.net.Uri;
import android.os.Build;
import android.os.Handler;
import android.os.Looper;
import android.provider.MediaStore;
import android.text.TextUtils;
import android.view.Display;
import android.view.WindowManager;

import java.lang.reflect.Method;
import java.util.ArrayList;
import java.util.List;

/**
 * Created by yangjie on 2017/6/5.
 */

public class ScreenShotListenerManager {

    // 读取媒体数据库时需要读取的列
    private static final String [] MEDIA_PROJECTIONS = {
            MediaStore.Images.ImageColumns.DATA,
            MediaStore.Images.ImageColumns.DATE_TAKEN
    };

    // 在API16以后可以获取WIDTH和HEIGHT
    private static final String[] MEDIA_PROJECTIONS_API_16 = {
            MediaStore.Images.ImageColumns.DATA,
            MediaStore.Images.ImageColumns.DATE_TAKEN,
            MediaStore.Images.ImageColumns.WIDTH,
            MediaStore.Images.ImageColumns.HEIGHT
    };

    // 截屏依据中的路径判断关键字
    private static final String[] KEYWORDS = {
            "screenshot","screencap"
    };

    private static Point mScreenRealSize;

    // 已回调过的路径
    private final List<String> mHasCallBackPaths = new ArrayList<String>();

    private Context mContext;

    private OnScreenShotListener mListener;

    private long mStartListenTime;

    // 内部存储器内容观察者
    private MediaContentObserver mInternalObserver;

    // 外部存储器内容观察者
    private MediaContentObserver mExternalObserver;


    private final Handler mUiHandler = new Handler(Looper.getMainLooper());


    private ScreenShotListenerManager(Context context) {
        mContext = context;

        if (mScreenRealSize == null) {
            mScreenRealSize = getRealScreenSize();
        }
    }

    // 获取屏幕的分辨率
    @TargetApi(Build.VERSION_CODES.JELLY_BEAN_MR1) 
    private Point getRealScreenSize() {
        Point screenSize = null;
        try {
            screenSize = new Point();
            WindowManager windowManager = (WindowManager) mContext.getSystemService(Context.WINDOW_SERVICE);
            Display defaultDisplay = windowManager.getDefaultDisplay();
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.JELLY_BEAN_MR1) {
                defaultDisplay.getRealSize(screenSize);
            } 
            else {
                try {
                    Method mGetRawW = Display.class.getMethod("getRawWidth");
                    Method mGetRawH = Display.class.getMethod("getRawHeight");
                    screenSize.set(
                            (Integer) mGetRawW.invoke(defaultDisplay),
                            (Integer) mGetRawH.invoke(defaultDisplay)
                    );
                } catch (Exception e) {
                    screenSize.set(defaultDisplay.getWidth(), defaultDisplay.getHeight());
                    e.printStackTrace();
                }
            }
        } catch (Exception e) {
            e.printStackTrace();
        }
        return screenSize;
    }


    protected static  ScreenShotListenerManager newInstance(Context context) {
        assertInMainThread();
        return new ScreenShotListenerManager(context);
    }

    private static void assertInMainThread() {
        if (Looper.myLooper() != Looper.getMainLooper()) {
            StackTraceElement[] elements = Thread.currentThread().getStackTrace();
            String methodMsg = null;
            if (elements != null && elements.length >= 4) {
                methodMsg = elements[3].toString();
            }
            throw new IllegalStateException("Call the method must be in main thread: " + methodMsg);
        }
    }

    public void startListen() {
        assertInMainThread();

        mHasCallBackPaths.clear();

        // 记录开始监听的时间戳
        mStartListenTime = System.currentTimeMillis();

        // 创建内容观察者
        mInternalObserver = new MediaContentObserver(MediaStore.Images.Media.INTERNAL_CONTENT_URI, mUiHandler);
        mExternalObserver = new MediaContentObserver(MediaStore.Images.Media.EXTERNAL_CONTENT_URI, mUiHandler);

        // 注册内容观察者
        mContext.getContentResolver().registerContentObserver(MediaStore.Images.Media.INTERNAL_CONTENT_URI, false, mInternalObserver);
        mContext.getContentResolver().registerContentObserver(MediaStore.Images.Media.EXTERNAL_CONTENT_URI, false, mExternalObserver);

    }

    // 停止监听
    public void stopListen() {
        assertInMainThread();

        //  注销内容观察者
        if (mInternalObserver != null) {
            try {
                mContext.getContentResolver().unregisterContentObserver(mInternalObserver);
            } catch (Exception e) {
                e.printStackTrace();
            }
            mInternalObserver = null;
        }

        if (mExternalObserver != null) {
            try {
                mContext.getContentResolver().unregisterContentObserver(mExternalObserver);
            } catch (Exception e) {
                e.printStackTrace();
            }
            mExternalObserver = null;
        }

        // 清空数据
        mStartListenTime = 0;
        mHasCallBackPaths.clear();

    }


    // 媒体内容观察者，观察媒体数据的变化
    class MediaContentObserver extends ContentObserver {
        private Uri mContentUri;

        public MediaContentObserver(Uri uri, Handler handler) {
            super(handler);
            mContentUri = uri;
        }

        @Override
        public void onChange(boolean selfChange) {
            super.onChange(selfChange);
            handleMediaContentChange(mContentUri);
        }
    }

    private void handleMediaContentChange(Uri mContentUri) {
        Cursor cursor = null;

        try {
            if (Build.VERSION.SDK_INT >= 24) {

            }
            cursor = mContext.getContentResolver().query(mContentUri, Build.VERSION.SDK_INT < 16 ? MEDIA_PROJECTIONS : MEDIA_PROJECTIONS_API_16, null, null, MediaStore.Images.ImageColumns.DATE_ADDED + " desc limit 1");

            if (cursor == null) return;
            if (!cursor.moveToFirst()) return;

            // 获取各列的索引
            int dataIndex = cursor.getColumnIndex(MediaStore.Images.ImageColumns.DATA);
            int dateTakenIndex = cursor.getColumnIndex(MediaStore.Images.ImageColumns.DATE_TAKEN);
            int widthIndex = -1;
            int heightIndex = -1;

            if (Build.VERSION.SDK_INT >= 16) {
                widthIndex = cursor.getColumnIndex(MediaStore.Images.ImageColumns.WIDTH);
                heightIndex = cursor.getColumnIndex(MediaStore.Images.ImageColumns.HEIGHT);
            }

            // 获取行数据
            String data = cursor.getString(dataIndex);
            long dateTaken = cursor.getLong(dateTakenIndex);
            int width = 0;
            int height = 0;
            if (widthIndex >= 0 && heightIndex >= 0) {
                width = cursor.getInt(widthIndex);
                height = cursor.getInt(heightIndex);
            } else {
                // API16之前, 只能手动获取
                Point size = getImageSize(data);
                width = size.x;
                height = size.y;
            }

            handleMediaRowData(data, dateTaken, width, height);
        } catch (Exception e) {
            e.printStackTrace();
        } finally {
            if (cursor != null && !cursor.isClosed()) {
                cursor.close();
            }
        }
    }

    private void handleMediaRowData(String data, long dateTaken, int width, int height) {
        if (checkScreenShot(data, dateTaken, width, height)) {
            if (mListener != null && !checkCallback(data)) {
                mListener.onShot(data);
            }
        }
    }

    // 判断是否已经回调过了，某些手机ROM截屏一次会发出多次内容改变的通知
    // 删除一个图片也会发送通知,同时防止删除图片时误将上一张符合截屏规则的图片当做截屏了
    private boolean checkCallback(String imagePath) {
        if(mHasCallBackPaths.contains(imagePath)) {
            return true;
        }

        // 缓存15~20条记录
        if (mHasCallBackPaths.size() >= 20) {
            for (int i = 0; i < 5; i++) {
                mHasCallBackPaths.remove(0);
            }
        }
        mHasCallBackPaths.add(imagePath);
        return false;
    }

    // 判断指定的数据是否满足截屏的条件
    private boolean checkScreenShot(String data, long dateTaken, int width, int height) {
        // 时间
        if (dateTaken < mStartListenTime || (System.currentTimeMillis() - dateTaken) > 10*1000){
            return false;
        }
        // 尺寸
        if (mScreenRealSize != null) {
            // 图片不能大于屏幕
            if (!(width <= mScreenRealSize.x && height <= mScreenRealSize.y || width <= mScreenRealSize.y && height <= mScreenRealSize.x)) {
                return false;
            }
        }
        // 路径
        if (TextUtils.isEmpty(data)) {
            return false;
        }
        data = data.toLowerCase().replaceAll("-", "").replaceAll("_","").replaceAll(" ","");
        // 判断路径是否含关键字
        for (String keyWork: KEYWORDS) {
            if (data.contains(keyWork)) {
                return true;
            }
        }
        return false;
    }

    private Point getImageSize(String imagePath) {
        BitmapFactory.Options options = new BitmapFactory.Options();
        options.inJustDecodeBounds = true;
        BitmapFactory.decodeFile(imagePath, options);
        return new Point(options.outWidth, options.outHeight);
    }

    /**
     * 设置截屏监听器
     */
    public void setListener(OnScreenShotListener listener) {
        mListener = listener;
    }

    public static interface OnScreenShotListener {
        public void onShot(String imagePath);
    }

}