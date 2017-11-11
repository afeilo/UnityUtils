//
//  AppDelegate.m
//  test
//
//  Created by 谢飞 on 2017/7/20.
//  Copyright © 2017年 chen. All rights reserved.
//

#import "AppDelegate.h"
#import "TSShareHelper.h"
#import "STAlertView.h"

extern "C"{
    /**
     *兼容 ios10以下 ios10采用异步跳转app
     *ios10以上应用跳转失败通过url跳转到网页
     */
    bool _jumpToApp(const char *uri,const char *webUrl){
        NSString *uriStr = [NSString stringWithUTF8String:uri];
        NSURL *appURL = [NSURL URLWithString:uriStr];
        UIApplication *application = [UIApplication sharedApplication];
        if ([application respondsToSelector:@selector(openURL:options:completionHandler:)]) {
            NSDictionary *options = @{UIApplicationOpenURLOptionUniversalLinksOnly : @YES};
            [application openURL:appURL options:options
               completionHandler:^(BOOL success) {
                   NSLog(@"IOS 10 Open %@: %d",uriStr,success);
                   if(!success){
                       NSString *webUrlStr = [NSString stringWithUTF8String:webUrl];
                       NSURL *webURL = [NSURL URLWithString:webUrlStr];
                       [application openURL:webURL options:@{} completionHandler:nil];
                   }
               }];
            return true;
        } else {
            BOOL success = [application openURL:appURL];
            NSLog(@"Below IOS 10 Open %@: %d",uriStr,success);
            return success;
        }
        // NSString *uriStr = [NSString stringWithUTF8String:uri];
        // NSURL *appURL = [NSURL URLWithString:uriStr];
        // if ([[UIApplication sharedApplication] canOpenURL:appURL]) {
        //     [[UIApplication sharedApplication] openURL:appURL];
        //     return true;
        // }
        // return false;
    }
}

@interface AppDelegate ()

@end

@implementation AppDelegate


- (BOOL)application:(UIApplication *)application didFinishLaunchingWithOptions:(NSDictionary *)launchOptions {
    // Override point for customization after application launch.
    return YES;
}


- (void)applicationWillResignActive:(UIApplication *)application {
    // Sent when the application is about to move from active to inactive state. This can occur for certain types of temporary interruptions (such as an incoming phone call or SMS message) or when the user quits the application and it begins the transition to the background state.
    // Use this method to pause ongoing tasks, disable timers, and invalidate graphics rendering callbacks. Games should use this method to pause the game.
}


- (void)applicationDidEnterBackground:(UIApplication *)application {
    // Use this method to release shared resources, save user data, invalidate timers, and store enough application state information to restore your application to its current state in case it is terminated later.
    // If your application supports background execution, this method is called instead of applicationWillTerminate: when the user quits.
        printf("applicationDidEnterBackground");
        [[NSNotificationCenter defaultCenter]removeObserver:self];
}


- (void)applicationWillEnterForeground:(UIApplication *)application {
    // Called as part of the transition from the background to the active state; here you can undo many of the changes made on entering the background.
}


- (void)applicationDidBecomeActive:(UIApplication *)application {
    // Restart any tasks that were paused (or not yet started) while the application was inactive. If the application was previously in the background, optionally refresh the user interface.
        printf("applicationDidBecomeActive");
        [[NSNotificationCenter defaultCenter]removeObserver:self];
        [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(screenShot) name:UIApplicationUserDidTakeScreenshotNotification object:nil];
}

- (void)applicationWillTerminate:(UIApplication *)application {
    // Called when the application is about to terminate. Save data if appropriate. See also applicationDidEnterBackground:.
}
- (void) screenShot{
    //人为截屏, 模拟用户截屏行为, 获取所截图片
    UIImage *image_ = [self imageWithScreenshot];
    [self tip:image_];
    //添加显示
//    UIImageView *imgvPhoto = [[UIImageView alloc]initWithImage:image_];
//    imgvPhoto.frame = CGRectMake(self.window.frame.size.width/2,0, self.window.frame.size.width/2, self.window.frame.size.height/2);
//    
//    //添加边框
//    CALayer * layer = [imgvPhoto layer];
//    layer.borderColor = [
//                         [UIColor whiteColor] CGColor];
//    layer.borderWidth = 5.0f;
//    //添加四个边阴影
//    imgvPhoto.layer.shadowColor = [UIColor blackColor].CGColor;
//    imgvPhoto.layer.shadowOffset = CGSizeMake(0, 0);
//    imgvPhoto.layer.shadowOpacity = 0.5;
//    imgvPhoto.layer.shadowRadius = 10.0;
//    //添加两个边阴影
//    imgvPhoto.layer.shadowColor = [UIColor blackColor].CGColor;
//    imgvPhoto.layer.shadowOffset = CGSizeMake(4, 4);
//    imgvPhoto.layer.shadowOpacity = 0.5;
//    imgvPhoto.layer.shadowRadius = 2.0;
//    
//    [self.window addSubview:imgvPhoto];
    

}

- (void)tip: (UIImage*) image{
    NSArray *buttons = nil;
    buttons = @[@"分享"];
    STAlertView *alert = [[STAlertView alloc] initWithTitle:@"截屏分享"
                                                      image:image
                                                    message:@""
                                               buttonTitles:buttons];
    
    alert.hideWhenTapOutside = YES;
    [alert setDidShowHandler:^{
        NSLog(@"显示了");
    }];
    [alert setDidHideHandler:^{
        NSLog(@"消失了");
    }];
    [alert setActionHandler:^(NSInteger index) {
        switch (index) {
            case 0:
            {
                    [TSShareHelper shareWithType:TSShareHelperShareTypeOthers  andController:self.window.rootViewController andItems:@[image]];
                break;
            }
            default:
                break;
        }
    }];
    [alert show:YES];
}

/**
 *  截取当前屏幕
 *
 *  @return NSData *
 */
- (NSData *)dataWithScreenshotInPNGFormat
{
    CGSize imageSize = CGSizeZero;
    UIInterfaceOrientation orientation = [UIApplication sharedApplication].statusBarOrientation;
    if (UIInterfaceOrientationIsPortrait(orientation))
        imageSize = [UIScreen mainScreen].bounds.size;
    else
        imageSize = CGSizeMake([UIScreen mainScreen].bounds.size.height, [UIScreen mainScreen].bounds.size.width);
    
    UIGraphicsBeginImageContextWithOptions(imageSize, NO, 0);
    CGContextRef context = UIGraphicsGetCurrentContext();
    for (UIWindow *window in [[UIApplication sharedApplication] windows])
    {
        CGContextSaveGState(context);
        CGContextTranslateCTM(context, window.center.x, window.center.y);
        CGContextConcatCTM(context, window.transform);
        CGContextTranslateCTM(context, -window.bounds.size.width * window.layer.anchorPoint.x, -window.bounds.size.height * window.layer.anchorPoint.y);
        if (orientation == UIInterfaceOrientationLandscapeLeft)
        {
            CGContextRotateCTM(context, M_PI_2);
            CGContextTranslateCTM(context, 0, -imageSize.width);
        }
        else if (orientation == UIInterfaceOrientationLandscapeRight)
        {
            CGContextRotateCTM(context, -M_PI_2);
            CGContextTranslateCTM(context, -imageSize.height, 0);
        } else if (orientation == UIInterfaceOrientationPortraitUpsideDown) {
            CGContextRotateCTM(context, M_PI);
            CGContextTranslateCTM(context, -imageSize.width, -imageSize.height);
        }
        if ([window respondsToSelector:@selector(drawViewHierarchyInRect:afterScreenUpdates:)])
        {
            [window drawViewHierarchyInRect:window.bounds afterScreenUpdates:YES];
        }
        else
        {
            [window.layer renderInContext:context];
        }
        CGContextRestoreGState(context);
    }
    
    UIImage *image = UIGraphicsGetImageFromCurrentImageContext();
    UIGraphicsEndImageContext();
    
    return UIImagePNGRepresentation(image);
}

-(UIImage*)imageWithScreenshot{
    NSData *imageData = [self dataWithScreenshotInPNGFormat];
    return [UIImage imageWithData:imageData];
}


@end
