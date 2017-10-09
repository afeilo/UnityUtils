package main;

import java.io.File;

import main.ScreenShotListenerManager.OnScreenShotListener;
import android.app.AlertDialog;
import android.content.Context;
import android.content.DialogInterface;
import android.content.DialogInterface.OnClickListener;
import android.content.Intent;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.net.Uri;
import android.widget.ImageView;

public class ScreenShot {
	private ScreenShotListenerManager manager;

	public ScreenShot(final Context context) {
		manager = ScreenShotListenerManager.newInstance(context);
		manager.setListener(new OnScreenShotListener() {

			@Override
			public void onShot(final String imagePath) {
				// TODO Auto-generated method stub
				BitmapFactory.Options options = new BitmapFactory.Options();
				options.inSampleSize = 2;
				Bitmap bitmap = BitmapFactory.decodeFile(imagePath, options);
				ImageView imageView = new ImageView(context);
				imageView.setImageBitmap(bitmap);
				new AlertDialog.Builder(context).setView(imageView)
						.setNegativeButton("·ÖÏí", new OnClickListener() {

							@Override
							public void onClick(DialogInterface arg0, int arg1) {
								// TODO Auto-generated method stub
								Intent intent2 = new Intent(Intent.ACTION_SEND);
								Uri uri = Uri.fromFile(new File(imagePath));

								intent2.putExtra(Intent.EXTRA_STREAM, uri);
								intent2.setType("image/*");
								context.startActivity(Intent.createChooser(
										intent2, "·ÖÏí"));
							}
						}).show();

			}
		});
	}

	public void onStart() {
		manager.startListen();
	}

	public void onStop() {
		manager.stopListen();
	}
}
