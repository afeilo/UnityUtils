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

    // ��ȡý�����ݿ�ʱ��Ҫ��ȡ����
    private static final String [] MEDIA_PROJECTIONS = {
            MediaStore.Images.ImageColumns.DATA,
            MediaStore.Images.ImageColumns.DATE_TAKEN
    };

    // ��API16�Ժ���Ի�ȡWIDTH��HEIGHT
    private static final String[] MEDIA_PROJECTIONS_API_16 = {
            MediaStore.Images.ImageColumns.DATA,
            MediaStore.Images.ImageColumns.DATE_TAKEN,
            MediaStore.Images.ImageColumns.WIDTH,
            MediaStore.Images.ImageColumns.HEIGHT
    };

    // ���������е�·���жϹؼ���
    private static final String[] KEYWORDS = {
            "screenshot","screencap"
    };

    private static Point mScreenRealSize;

    // �ѻص�����·��
    private final List<String> mHasCallBackPaths = new ArrayList<String>();

    private Context mContext;

    private OnScreenShotListener mListener;

    private long mStartListenTime;

    // �ڲ��洢�����ݹ۲���
    private MediaContentObserver mInternalObserver;

    // �ⲿ�洢�����ݹ۲���
    private MediaContentObserver mExternalObserver;


    private final Handler mUiHandler = new Handler(Looper.getMainLooper());


    private ScreenShotListenerManager(Context context) {
        mContext = context;

        if (mScreenRealSize == null) {
            mScreenRealSize = getRealScreenSize();
        }
    }

    // ��ȡ��Ļ�ķֱ���
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

        // ��¼��ʼ������ʱ���
        mStartListenTime = System.currentTimeMillis();

        // �������ݹ۲���
        mInternalObserver = new MediaContentObserver(MediaStore.Images.Media.INTERNAL_CONTENT_URI, mUiHandler);
        mExternalObserver = new MediaContentObserver(MediaStore.Images.Media.EXTERNAL_CONTENT_URI, mUiHandler);

        // ע�����ݹ۲���
        mContext.getContentResolver().registerContentObserver(MediaStore.Images.Media.INTERNAL_CONTENT_URI, false, mInternalObserver);
        mContext.getContentResolver().registerContentObserver(MediaStore.Images.Media.EXTERNAL_CONTENT_URI, false, mExternalObserver);

    }

    // ֹͣ����
    public void stopListen() {
        assertInMainThread();

        //  ע�����ݹ۲���
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

        // �������
        mStartListenTime = 0;
        mHasCallBackPaths.clear();

    }


    // ý�����ݹ۲��ߣ��۲�ý�����ݵı仯
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

            // ��ȡ���е�����
            int dataIndex = cursor.getColumnIndex(MediaStore.Images.ImageColumns.DATA);
            int dateTakenIndex = cursor.getColumnIndex(MediaStore.Images.ImageColumns.DATE_TAKEN);
            int widthIndex = -1;
            int heightIndex = -1;

            if (Build.VERSION.SDK_INT >= 16) {
                widthIndex = cursor.getColumnIndex(MediaStore.Images.ImageColumns.WIDTH);
                heightIndex = cursor.getColumnIndex(MediaStore.Images.ImageColumns.HEIGHT);
            }

            // ��ȡ������
            String data = cursor.getString(dataIndex);
            long dateTaken = cursor.getLong(dateTakenIndex);
            int width = 0;
            int height = 0;
            if (widthIndex >= 0 && heightIndex >= 0) {
                width = cursor.getInt(widthIndex);
                height = cursor.getInt(heightIndex);
            } else {
                // API16֮ǰ, ֻ���ֶ���ȡ
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

    // �ж��Ƿ��Ѿ��ص����ˣ�ĳЩ�ֻ�ROM����һ�λᷢ��������ݸı��֪ͨ
    // ɾ��һ��ͼƬҲ�ᷢ��֪ͨ,ͬʱ��ֹɾ��ͼƬʱ����һ�ŷ��Ͻ��������ͼƬ����������
    private boolean checkCallback(String imagePath) {
        if(mHasCallBackPaths.contains(imagePath)) {
            return true;
        }

        // ����15~20����¼
        if (mHasCallBackPaths.size() >= 20) {
            for (int i = 0; i < 5; i++) {
                mHasCallBackPaths.remove(0);
            }
        }
        mHasCallBackPaths.add(imagePath);
        return false;
    }

    // �ж�ָ���������Ƿ��������������
    private boolean checkScreenShot(String data, long dateTaken, int width, int height) {
        // ʱ��
        if (dateTaken < mStartListenTime || (System.currentTimeMillis() - dateTaken) > 10*1000){
            return false;
        }
        // �ߴ�
        if (mScreenRealSize != null) {
            // ͼƬ���ܴ�����Ļ
            if (!(width <= mScreenRealSize.x && height <= mScreenRealSize.y || width <= mScreenRealSize.y && height <= mScreenRealSize.x)) {
                return false;
            }
        }
        // ·��
        if (TextUtils.isEmpty(data)) {
            return false;
        }
        data = data.toLowerCase().replaceAll("-", "").replaceAll("_","").replaceAll(" ","");
        // �ж�·���Ƿ񺬹ؼ���
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
     * ���ý���������
     */
    public void setListener(OnScreenShotListener listener) {
        mListener = listener;
    }

    public static interface OnScreenShotListener {
        public void onShot(String imagePath);
    }

}