package com.test;

import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.io.InputStream;
import java.math.BigInteger;
import java.security.MessageDigest;
import java.security.NoSuchAlgorithmException;
import java.util.Vector;

import org.xmlpull.v1.XmlPullParser;
import org.xmlpull.v1.XmlPullParserFactory;

import android.app.Activity;
import android.content.Context;
import android.content.pm.PackageManager;
import android.os.Bundle;
import android.os.Environment;
import android.util.Log;

public class ObbCheck {
	
	/**
	 *  检验obb是否合法
	 * @param unityPlayer
	 * @return
	 */
	
	public static boolean checkMd5(Activity unityPlayer){
		Bundle settings = getXml(unityPlayer);
		if(settings.containsKey("useObb")&&!settings.getBoolean("useObb")){
			return true; 
		}

		String[] paths = getObbPath(unityPlayer);
		for (String string : paths) {
			if(settings.getBoolean(getMd5(string))){
				return true;
			}
		}
		return false;
	}


	/**
	 * 通过obb文件获取加密MD5
	 * @param paramString
	 * @return
	 */
	private static String getMd5(String paramString) {
		try {
			Log.d("ObbCheck", "path = " + paramString);
			MessageDigest localMessageDigest = MessageDigest.getInstance("MD5");
			FileInputStream localFileInputStream = new FileInputStream(
					paramString);
			long lenght = new File(paramString).length();
			localFileInputStream.skip(lenght - Math.min(lenght, 65558L));
			byte[] arrayOfByte = new byte[1024];
			for (int i2 = 0; i2 != -1; i2 = localFileInputStream
					.read(arrayOfByte)) {
				localMessageDigest.update(arrayOfByte, 0, i2);
			}
			BigInteger bi = new BigInteger(1, localMessageDigest.digest());
			Log.d("ObbCheck", "md5 = " + bi.toString(16));
			return bi.toString(16);
		} catch (FileNotFoundException localFileNotFoundException) {
		} catch (IOException localIOException) {
		} catch (NoSuchAlgorithmException localNoSuchAlgorithmException) {

		}
		return null;
	}

	/**
	 * 获取应用obb位置
	 * @param paramContext
	 * @return
	 */
	private static String[] getObbPath(Context paramContext) {
		String str1 = paramContext.getPackageName();
		Vector<String> localVector = new Vector<String>();
		try {
			int i1 = paramContext.getPackageManager().getPackageInfo(str1, 0).versionCode;
			if (Environment.getExternalStorageState().equals("mounted")) {
				File localFile1 = Environment.getExternalStorageDirectory();
				File localFile2 = new File(localFile1.toString()
						+ "/Android/obb/" + str1);
				if (localFile2.exists()) {
					if (i1 > 0) {
						String str3 = localFile2 + File.separator + "main."
								+ i1 + "." + str1 + ".obb";
						if (new File(str3).isFile()) {
							localVector.add(str3);
						}
					}
					if (i1 > 0) {
						String str2 = localFile2 + File.separator + "patch."
								+ i1 + "." + str1 + ".obb";
						if (new File(str2).isFile()) {
							localVector.add(str2);
						}
					}
				}
			}
			String[] arrayOfString = new String[localVector.size()];
			localVector.toArray(arrayOfString);
			return arrayOfString;
		} catch (PackageManager.NameNotFoundException localNameNotFoundException) {
		}
		return new String[0];
	}
	
	/**
	 * 读取Obb配置表
	 * @param context
	 * @return
	 */
	private static Bundle getXml(Context context) {
		Bundle bundle = new Bundle();
		XmlPullParser localXmlPullParser;
		// int i1;
		String str;
		try {
			File localFile = new File(context.getPackageCodePath(),
					"assets/bin/Data/settings.xml");
			Object localObject1;
			if (localFile.exists())

				localObject1 = new FileInputStream(localFile);
			else
				localObject1 = context.getAssets()
						.open("bin/Data/settings.xml");

			XmlPullParserFactory localXmlPullParserFactory = XmlPullParserFactory
					.newInstance();
			localXmlPullParserFactory.setNamespaceAware(true);
			localXmlPullParser = localXmlPullParserFactory.newPullParser();
			localXmlPullParser.setInput((InputStream) localObject1,null);
			int type = localXmlPullParser.getEventType();
			Object localObject2 = null;
			str = null;
//			int i = 0;
			while (type!=1) {
				switch (type) {
				case 2:
					if (localXmlPullParser.getAttributeCount()==0) {
						type = localXmlPullParser.next();
						continue;
					}
					str = localXmlPullParser.getName();
					localObject2 = localXmlPullParser.getAttributeName(0);
					if (!localXmlPullParser.getAttributeName(0).equals("name")){
						type = localXmlPullParser.next();
						continue;
						}
					localObject2 = localXmlPullParser.getAttributeValue(0);
					if (str.equalsIgnoreCase("integer")) {
						bundle.putInt((String) localObject2,
								Integer.parseInt(localXmlPullParser.nextText()));
					} else if (str.equalsIgnoreCase("string")) {
						bundle.putString((String) localObject2,
								localXmlPullParser.nextText());
					} else if (str.equalsIgnoreCase("bool")) {
						bundle.putBoolean((String) localObject2, Boolean
								.parseBoolean(localXmlPullParser.nextText()));
					} else if (str.equalsIgnoreCase("float")) {
						bundle.putFloat((String) localObject2,
								Float.parseFloat(localXmlPullParser.nextText()));
					}
					break;
				default:
					break;
				}
//				i++;
				type = localXmlPullParser.next();
			}

		} catch (Exception localException) {
			localException.printStackTrace();
		}
		return bundle;
	}
}

