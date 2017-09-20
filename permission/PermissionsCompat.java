package main;

import java.util.ArrayList;
import java.util.List;

import android.annotation.TargetApi;
import android.app.Activity;
import android.content.Context;
import android.content.pm.PackageInfo;
import android.content.pm.PackageManager;
import android.content.pm.PackageManager.NameNotFoundException;
import android.content.pm.PermissionInfo;
import android.support.v4.app.ActivityCompat;
import android.support.v4.content.ContextCompat;
import android.util.Log;

@TargetApi(23) 
public final class PermissionsCompat {
	//授权成功编码
	public static final int SUCCESS_CODE = 0;
	//授权失败编码
	public static final int FAIL_CODE = 1;
	//拒绝授权编码（下一次不再显示）
	public static final int REJEST_CODE =2;
	
	
	public static void requestPermissions(Activity activity,int requestCode,
			String... permissions) {
//		Activity activity = mActivity.get();
		if(activity==null)
			return;
		_requestPermissions(activity,requestCode, permissions);
	}
	
	/**
	 * 调用该方法申请应用所有的权限
	 * @param activity
	 * @param requestCode
	 */
	public static void requestAllPermissions(Activity activity,int requestCode){
//		Activity activity = mActivity.get();
		if(activity==null)
			return;
		String[] permissions = loadAllPermissions(activity);
		if(permissions!=null){
			_requestPermissions(activity,requestCode, permissions);
		}
	}

	/**
	 * 加载Manifest所有权限
	 * @return
	 */
	private static String[] loadAllPermissions(Activity activity){
		if (activity == null) {
			return null;
		}
		PackageManager localPackageManager = activity.getPackageManager();
		PackageInfo localPackageInfo = null;
		try {
			localPackageInfo = localPackageManager.getPackageInfo(
					activity.getPackageName(), 4096);

		} catch (NameNotFoundException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
			return null;
		}
		return localPackageInfo.requestedPermissions;
	}

	/**
	 * 检测权限弹出框是否需要添加，如果不需要则不用弹出
	 * @param str
	 * @return
	 */
	private static boolean checkPermission(Context context,String str){
		PackageManager localPackageManager = context.getPackageManager();
			try {
				if ((localPackageManager.getPermissionInfo(str, 128).protectionLevel == PermissionInfo.PROTECTION_DANGEROUS)
						&& (ContextCompat.checkSelfPermission(context,str) != PackageManager.PERMISSION_GRANTED)){
					Log.d("PermissionUtil",
							"add permission info  ");
					return true;
				}
					
			} catch (NameNotFoundException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
				Log.d("PermissionUtil",
						"Failed to get permission info for "
								+ str
								+ ", manifest likely missing custom permission declaration");
			}
		return false;
	}
	
	/**
	 * 获取权限 筛选权限
	 * 这里希望调用的时候按照权限组进行调用，每次申请的时候申请一个权限组的权限
	 * @param requestCode
	 * @param permissions
	 */
	private static void _requestPermissions(Activity activity,int requestCode,
			String... permissions) {
		if(activity==null)
			return;
		if (Utils.isOverMarshmallow()) {
			List<String> permissionList = new ArrayList<String>();
			for (int i = 0; i < permissions.length; i++) {
				if(checkPermission(activity,permissions[i])){
					permissionList.add(permissions[i]);
				}
//				else{
//					doExecuteCallBack(SUCCESS_CODE, requestCode, permissions[i]);
//				}
				
			}
			String[] array = new String[permissionList.size()];
			permissionList.toArray(array);
			if(array.length>0){
				for (int i = 0; i < array.length; i++) {
					Log.d("PermissionUtil", array[i]);
				}
				Log.d("PermissionUtil", "requestPermissions");
				ActivityCompat.requestPermissions(activity,array, requestCode);
			}
		}

	}

	private static void doExecuteCallBack(int resultCode,int requestCode,String permission,PermissionCallback callback) {
		if(callback!=null){
			callback.permissionCallback(resultCode, requestCode, permission);
		}
	}



	public static void onRequestPermissionsResult(Activity activity,int requestCode, String[] permissions, int[] grantResults,PermissionCallback callback) {
		requestResult(activity,requestCode, permissions, grantResults,callback);
	}


	private static void requestResult(Activity activity,int requestCode,
			String[] permissions, int[] grantResults,PermissionCallback callback) {
//		Activity activity = mActivity.get();
		if(activity == null)
			return;
		for (int i = 0; i < grantResults.length; i++) {
			if (grantResults[i] == PackageManager.PERMISSION_GRANTED) {
				doExecuteCallBack(SUCCESS_CODE, requestCode,permissions[i],callback);
			}else{
				if(ActivityCompat.shouldShowRequestPermissionRationale(activity,permissions[i])){
					doExecuteCallBack(FAIL_CODE, requestCode,permissions[i],callback);
				}else{
					doExecuteCallBack(REJEST_CODE, requestCode, permissions[i],callback);
				}
			}
		}
	}
	
	public interface PermissionCallback{
		void permissionCallback(int resultCode,int requestCode,String permission);
	}

}


/**how to use 
public class Test extends Activity implements PermissionCallback {
	private static final int REQUEST_CODE = 1000;
	@Override
	protected void onCreate(Bundle savedInstanceState) {
		// TODO Auto-generated method stub
		super.onCreate(savedInstanceState);
		// 会申请整个权限组，你只用填一个就够了，方便后面处理权限
		PermissionsCompat.requestPermissions(this, REQUEST_CODE,
				Manifest.permission.READ_PHONE_STATE,
			    Manifest.permission.RECORD_AUDIO,
				Manifest.permission.WRITE_EXTERNAL_STORAGE,
				Manifest.permission.ACCESS_FINE_LOCATION);
	}

	@Override
	public void permissionCallback(int resultCode, int requestCode,
			String permission) {
		// TODO Auto-generated method stub
		if (REQUEST_CODE != requestCode)
			return;

		switch (resultCode) {
		case PermissionsCompat.FAIL_CODE:
			permissionRequestFail(requestCode, permission);
			break;
		case PermissionsCompat.REJEST_CODE:
			permissionRequestRejest(requestCode, permission);
			break;
		default:
			break;
		}
	}

	private void permissionRequestFail(final int requestCode,
			final String permission) {
		if (Manifest.permission.WRITE_EXTERNAL_STORAGE.equals(permission)
				&& ContextCompat.checkSelfPermission(this, permission) != PackageManager.PERMISSION_GRANTED) {
			new AlertDialog.Builder(this)
					.setMessage("我们需要您手机的内存读取权限，否则游戏无法正常进入。游戏将会关闭！")
					.setPositiveButton("确定",
							new DialogInterface.OnClickListener() {
								@Override
								public void onClick(DialogInterface dialog,
										int which) {
									PermissionsCompat.requestPermissions(
											Test.this, requestCode, permission);
								}
							})
					.setNegativeButton("拒绝",
							new DialogInterface.OnClickListener() {

								@Override
								public void onClick(DialogInterface arg0,
										int arg1) {
									// TODO Auto-generated method stub
									finish();
								}
							}).show();

		}
	}

	private void permissionRequestRejest(int requestCode, String permission) {
		if (Manifest.permission.WRITE_EXTERNAL_STORAGE.equals(permission)
				&& ContextCompat.checkSelfPermission(this, permission) != PackageManager.PERMISSION_GRANTED) {
			new AlertDialog.Builder(this)
					.setMessage("您拒绝了授予权限，必须在设置界面开启权限，否则游戏无法正常进入。游戏将会关闭！")
					.setPositiveButton("设置",
							new DialogInterface.OnClickListener() {
								@Override
								public void onClick(DialogInterface dialog,
										int which) {
									Intent intent = new Intent(
											Settings.ACTION_APPLICATION_DETAILS_SETTINGS);
									intent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
									Uri uri = Uri.fromParts("package",
											getPackageName(), null);
									intent.setData(uri);
									startActivity(intent);
								}
							})
					.setNegativeButton("拒绝",
							new DialogInterface.OnClickListener() {

								@Override
								public void onClick(DialogInterface arg0,
										int arg1) {
									// TODO Auto-generated method stub
									finish();
								}
							}).show();

		}
	}

	@Override
	public void onRequestPermissionsResult(int requestCode,
			String[] permissions, int[] grantResults) {
		// TODO Auto-generated method stub
		PermissionsCompat.onRequestPermissionsResult(this, requestCode,
				permissions, grantResults, this);
	}

}

*/