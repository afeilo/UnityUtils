#if UNITY_ANDROID
public class ObbCheck
{
    
    //校验Obb
    public static bool CheckObb() {
        using (AndroidJavaClass unity_player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject current_activity = unity_player.GetStatic<AndroidJavaObject>("currentActivity");
            using(AndroidJavaClass obbCheck = new AndroidJavaClass("com.test.ObbCheck")){
                return obbCheck.CallStatic<bool>("checkMd5", current_activity);
            }
            
        }
    }
}
#endif