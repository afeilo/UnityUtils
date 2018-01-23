package com.mediaplayer;

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
import android.os.Debug;
import android.util.Log;
import android.view.Gravity;
import android.view.View;
import android.view.View.OnClickListener;
import android.view.ViewGroup;
import android.view.ViewGroup.LayoutParams;
import android.widget.FrameLayout;
import android.widget.ImageButton;

public class VideoPlayer implements OnCompletionListener,OnErrorListener,OnPreparedListener{
	private Context mContext = null;
	private Activity mActivity = null;
	private FrameLayout layout;
	private VideoView videoView;
	private ViewGroup contentView;//播放视频容器
	public VideoPlayer(Activity context,boolean canSkip){
		mContext = context;
		mActivity = context;
		initView(canSkip);
	}
	
	public void playAssetResFront(String name){
		View decorView = mActivity.getWindow().getDecorView();
		contentView = (ViewGroup) decorView.findViewById(android.R.id.content);
		contentView.addView(layout);
		playAssetRes(name);
		
	}
	
	public void playAssetRes(String name){
		try {
			videoView.setVideoFileDescriptor(mContext.getAssets().openFd(name));
			videoView.start();
		} catch (IOException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
			stop();
		}	
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
			ImageButton button = new ImageButton(mContext);
			params = new FrameLayout.LayoutParams(LayoutParams.WRAP_CONTENT, 
	    			LayoutParams.WRAP_CONTENT);
			params.gravity = Gravity.RIGHT;
			button.setLayoutParams(params);
			button.setBackgroundColor(Color.TRANSPARENT);
			button.setImageResource(getResourceId(mContext,"skip_button", "drawable", mContext.getPackageName()));
			button.setOnClickListener(new OnClickListener() {
				
				@Override
				public void onClick(View arg0) {
					// TODO Auto-generated method stub
					
					if (mActivity != null){
						mActivity.runOnUiThread(new Runnable() {
							@Override
							public void run() {
								
								// TODO Auto-generated method stub
								stop();
							}
						});
					}
				}
			});
			layout.addView(button);
		}


		
	}
	
	public void stop(){
		videoView.setZOrderMediaOverlay(false);
		if (contentView.getChildCount()>1)
			contentView.bringChildToFront(contentView.getChildAt(contentView.getChildCount()-2));
		videoView.stopPlayback();
		if(contentView != null)
			contentView.removeView(layout);
	}
	
	@Override
	public void onPrepared(MediaPlayer arg0) {
		// TODO Auto-generated method stub
//		contentView.bringChildToFront(layout);
	}

	
	@Override
	public boolean onError(MediaPlayer arg0, int arg1, int arg2) {
		// TODO 输入错误信息
		
		stop();
		return true;
	}

	@Override
	public void onCompletion(MediaPlayer arg0) {
		stop();
		
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
	public static void Play(final Activity activity,final String name,final boolean canSkip){
		activity.runOnUiThread(new Runnable() {
			
			@Override
			public void run() {
				// TODO Auto-generated method stub
				VideoPlayer player = new VideoPlayer(activity,canSkip);
				player.playAssetResFront(name);
			}
		});

	}



}
