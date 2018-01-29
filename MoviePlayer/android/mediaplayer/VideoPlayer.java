package main;

import java.io.IOException;

import android.app.Activity;
import android.content.Context;
import android.content.pm.PackageManager;
import android.content.pm.PackageManager.NameNotFoundException;
import android.content.res.Resources;
import android.graphics.Color;
import android.media.MediaPlayer;
import android.media.MediaPlayer.OnCompletionListener;
import android.media.MediaPlayer.OnErrorListener;
import android.media.MediaPlayer.OnPreparedListener;
import android.text.TextUtils;
import android.view.Gravity;
import android.view.MotionEvent;
import android.view.View;
import android.view.View.OnClickListener;
import android.view.View.OnTouchListener;
import android.view.ViewGroup;
import android.view.ViewGroup.LayoutParams;
import android.view.animation.AlphaAnimation;
import android.view.animation.Animation;
import android.widget.FrameLayout;
import android.widget.ImageView;

import com.unity3d.player.UnityPlayer;

public class VideoPlayer implements OnCompletionListener,OnErrorListener,OnPreparedListener,OnTouchListener{
	private Context mContext = null;
	private Activity mActivity = null;
	private FrameLayout layout;
	private VideoView videoView;
	private ViewGroup contentView;//播放视频容器
	private ImageView skipButton;//跳过anniu
	private String gameObjectName;//gameobjectName 用于发送通知
	private final String UNITY_CALLBACK = "PlayMovieFinish";
	public static int count = 0;
	private static VideoPlayer videoPlayer;
	
	public static VideoPlayer getInstance(Activity context){
		if(videoPlayer == null)
			videoPlayer = new VideoPlayer();
		videoPlayer.mActivity = context;
		videoPlayer.mContext = context;
		return videoPlayer;
	}
	
	private VideoPlayer(){
	}
	
		
	public void playAssetResFront(String name,boolean canSkip,String gameObjectName){
		if(videoView!=null)
			stop("play next");
		this.gameObjectName = gameObjectName;
		initView(canSkip);
		View decorView = mActivity.getWindow().getDecorView();
		contentView = (ViewGroup) decorView.findViewById(android.R.id.content);
		contentView.addView(layout);
		playAssetRes(name);
		
	}
	
	public void playAssetRes(String name){
		count ++;
		try {
			videoView.setVideoFileDescriptor(mContext.getAssets().openFd(name));
			videoView.start();
			
		} catch (IOException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
			stop(e.getMessage());
		}	
	}
	
	public int dip2px(Context context, float dipValue){ 
		final float scale = context.getResources().getDisplayMetrics().density; 
		return (int)(dipValue * scale + 0.5f); 
	} 
	
	private void initView(boolean canSkip){
		layout = new FrameLayout(mContext);
		layout.setLayoutParams(new LayoutParams(LayoutParams.MATCH_PARENT, 
    			LayoutParams.MATCH_PARENT));
		layout.setBackgroundColor(Color.BLACK);
		videoView = new VideoView(mContext);
		FrameLayout.LayoutParams params = new FrameLayout.LayoutParams(LayoutParams.MATCH_PARENT, 
				LayoutParams.MATCH_PARENT);
		params.gravity = Gravity.CENTER;
		videoView.setLayoutParams(params);
		videoView.setOnCompletionListener(this);
		videoView.setOnErrorListener(this);
		videoView.setOnPreparedListener(this);
		videoView.setZOrderMediaOverlay(true);
		layout.addView(videoView);
		if (canSkip) {
			videoView.setOnTouchListener(this);
			skipButton = new ImageView(mContext);
			params = new FrameLayout.LayoutParams(dip2px(mContext,70), 
					dip2px(mContext,16));
			params.rightMargin =dip2px(mContext,16);
			params.topMargin = dip2px(mContext,16);
			params.gravity = Gravity.RIGHT;
			skipButton.setLayoutParams(params);
			skipButton.setImageResource(getResourceId(mContext,"skip_button", "drawable", mContext.getPackageName()));
			skipButton.setOnClickListener(new OnClickListener() {
				
				@Override
				public void onClick(View arg0) {
					// TODO Auto-generated method stub
					
					if (mActivity != null){
						mActivity.runOnUiThread(new Runnable() {
							@Override
							public void run() {
								
								// TODO Auto-generated method stub
								stop("skip");
							}
						});
					}
				}
			});
			layout.addView(skipButton);
			setAnimation(skipButton, 1);
		}


		
	}
	
	public void stop(String msg){
		count --;
		videoView.setZOrderMediaOverlay(false);
		if (contentView.getChildCount()>1)
			contentView.bringChildToFront(contentView.getChildAt(contentView.getChildCount()-2));
		videoView.stopPlayback();
		videoView = null;
		if(contentView != null)
			contentView.removeView(layout);
		if(!TextUtils.isEmpty(gameObjectName)){
			UnityPlayer.UnitySendMessage(gameObjectName, UNITY_CALLBACK ,msg);
		}
	}
	
	@Override
	public void onPrepared(MediaPlayer arg0) {
		// TODO Auto-generated method stub
//		contentView.bringChildToFront(layout);
	}

	@Override
	public boolean onTouch(View arg0, MotionEvent event) {
		// TODO Auto-generated method stub

		int action = event.getAction();
		if(action == MotionEvent.ACTION_DOWN){
			setAnimation(skipButton,500);
			return true;
		}
		return false;
	}
	AlphaAnimation mShowAnimation;
	
	private void setAnimation( final View view, int duration ){  
	    if( null == view || duration < 0 ){  
	        return;  
	    }  
	    if( null != mShowAnimation ){  
	    	mShowAnimation.cancel( );  
	    }  
	    if(view.isClickable())
	    	mShowAnimation = new AlphaAnimation(1.0f, 0.0f);  
	    else 
	    	mShowAnimation = new AlphaAnimation(0f, 1.0f);  
	    mShowAnimation.setDuration( duration );  
	    mShowAnimation.setFillAfter( true );  
	    mShowAnimation.setAnimationListener(new Animation.AnimationListener() {
			
			@Override
			public void onAnimationStart(Animation arg0) {
				// TODO Auto-generated method stub
				view.setClickable(!view.isClickable());
			}
			
			@Override
			public void onAnimationRepeat(Animation arg0) {
				// TODO Auto-generated method stub
				
			}
			
			@Override
			public void onAnimationEnd(Animation arg0) {
				// TODO Auto-generated method stub
					
			}
		});
	    view.startAnimation( mShowAnimation );  
	}  
	
	@Override
	public boolean onError(MediaPlayer arg0, int arg1, int arg2) {
		// TODO 输入错误信息
		
		stop("onError");
		return true;
	}

	@Override
	public void onCompletion(MediaPlayer arg0) {
		stop("onCompletion");
		
	}
	
	/**
	 * getIdentifier("icon", "drawable", "org.anddev.android.testproject");
	 * 获取资源 绕过R
	 * @param context
	 * @param name
	 * @param type
	 * @param packageName
	 * @return
	 */
	private int getResourceId(Context context,String name,String type,String packageName){
		 
	      Resources themeResources=null;
	      PackageManager pm=context.getPackageManager();
	       try {
	         themeResources=pm.getResourcesForApplication(packageName);
	          return themeResources.getIdentifier(name, type, packageName);
	       } catch(NameNotFoundException e) {

	         e.printStackTrace();
	       }
	       return 0;
	 }
	
	/**
	 * 
	 * @param activity
	 * @param name
	 */
	public static void Play(final Activity activity,final String name,final String gameObjectName,final boolean canSkip){
		activity.runOnUiThread(new Runnable() {
			
			@Override
			public void run() {
				// TODO Auto-generated method stub
				VideoPlayer.getInstance(activity).playAssetResFront(name,canSkip,gameObjectName);
			}
		});

	}

	/**当前是否正在播放视频
	 * 
	 * @return
	 */
	public static boolean isMoviePlaying(){
		return count > 0;
	}

}
