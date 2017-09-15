package main;

import java.lang.ref.WeakReference;
import java.util.ArrayList;
import java.util.List;

import android.annotation.TargetApi;
import android.app.Activity;
import android.content.Context;
import android.content.pm.PackageInfo;
import android.content.pm.PackageManager;
import android.content.pm.PackageManager.NameNotFoundException;
import android.content.pm.PermissionInfo;
import android.util.Log;

@TargetApi(23) 
public final class PermissionsCompat {
	private WeakReference<Activity> mActivity;
	private WeakReference<PermissionCallback> callback;
	PackageManager localPackageManager;
	//授权成功编码
	public static int SUCCESS_CODE = 0;
	//授权失败编码
	public static int FAIL_CODE = 1;
	//拒绝授权编码（下一次不再显示）
	public static int REJEST_CODE =2;
	
	public PermissionsCompat(Activity activity){
		this(activity, null);
	}
	public PermissionsCompat(Activity activity,PermissionCallback callback){
		this.mActivity = new WeakReference<Activity>(activity);
		if(callback!=null){
			this.callback = new WeakReference<PermissionsCompat.PermissionCallback>(callback);
		}
		localPackageManager = activity.getPackageManager();
	}
	
	public void requestPermissions( int requestCode,
			String... permissions) {
		_requestPermissions(requestCode, permissions);
	}
	
	public void requestAllPermissions(int requestCode){
		String[] permissions = loadAllPermissions();
		if(permissions!=null){
			_requestPermissions(requestCode, permissions);
		}
	}

	/**
	 * 加载Manifest所有权限
	 * @return
	 */
	private String[] loadAllPermissions(){
		Activity paramActivity = mActivity.get();
		if (paramActivity == null) {
			return null;
		}
		PackageManager localPackageManager = paramActivity.getPackageManager();
		PackageInfo localPackageInfo = null;
		try {
			localPackageInfo = localPackageManager.getPackageInfo(
					paramActivity.getPackageName(), 4096);

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
	private boolean checkPermission(Context context,String str){
			try {
				if ((localPackageManager.getPermissionInfo(str, 128).protectionLevel == PermissionInfo.PROTECTION_DANGEROUS)
						&& (context.checkCallingOrSelfPermission(str) != PackageManager.PERMISSION_GRANTED)){
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
	 * @param requestCode
	 * @param permissions
	 */
	private void _requestPermissions(int requestCode,
			String... permissions) {
		Activity activity = mActivity.get();
		if(activity==null)
			return;
		if (Utils.isOverMarshmallow()) {
			List<String> permissionList = new ArrayList<String>();
			for (int i = 0; i < permissions.length; i++) {
				if(checkPermission(activity,permissions[i])){
					permissionList.add(permissions[i]);
				}else{
					doExecuteCallBack(SUCCESS_CODE, requestCode, permissions[i]);
				}
				
			}
			String[] array = new String[permissionList.size()];
			permissionList.toArray(array);
			if(mActivity.get()!=null&&array.length>0){
				for (int i = 0; i < array.length; i++) {
					Log.d("PermissionUtil", array[i]);
				}
				mActivity.get().requestPermissions(array, requestCode);
			}
			
		}

	}

	private void doExecuteCallBack(int resultCode,int requestCode,String permission) {
		if(callback.get()!=null){
			callback.get().permissionCallback(resultCode, requestCode, permission);
		}
	}



	public void onRequestPermissionsResult(int requestCode, String[] permissions, int[] grantResults) {
		requestResult(requestCode, permissions, grantResults);
	}


	private void requestResult(int requestCode,
			String[] permissions, int[] grantResults) {
		Activity activity = mActivity.get();
		if(activity == null)
			return;
		for (int i = 0; i < grantResults.length; i++) {
			if (grantResults[i] == PackageManager.PERMISSION_GRANTED) {
				doExecuteCallBack(SUCCESS_CODE, requestCode,permissions[i]);
			}else{
				if(activity.shouldShowRequestPermissionRationale(permissions[i])){
					doExecuteCallBack(FAIL_CODE, requestCode,permissions[i]);
				}else{
					doExecuteCallBack(REJEST_CODE, requestCode, permissions[i]);
				}
			}
		}
	}
	
	public interface PermissionCallback{
		void permissionCallback(int resultCode,int requestCode,String permission);
	}

}
