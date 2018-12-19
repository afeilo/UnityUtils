import android.os.Bundle;
import android.util.Log;
import android.view.Gravity;
import android.widget.FrameLayout;
import android.widget.ImageView;

import com.unity3d.player.UnityPlayerActivity;

public class SplashActivity extends UnityPlayerActivity{
	private ImageView splashImageView;
	@Override
	protected void onCreate(Bundle arg0) {
		// TODO Auto-generated method stub
		super.onCreate(arg0);
		loadingSplash();
	}
	/**
	 * ¼ÓÔØsplash
	 */
	private void loadingSplash(){
		splashImageView = new ImageView(this);
		splashImageView.setBackgroundResource(R.drawable.logo);
		FrameLayout.LayoutParams params = new FrameLayout.LayoutParams(FrameLayout.LayoutParams.WRAP_CONTENT  , FrameLayout.LayoutParams.WRAP_CONTENT, Gravity.CENTER);
		mUnityPlayer.addView(splashImageView, params);
	}
	public void hideSplash(){
		runOnUiThread(new Runnable() {
			
			@Override
			public void run() {
				// TODO Auto-generated method stub
				mUnityPlayer.removeView(splashImageView);
			}
		});
	}
	
}
