//
//  ViewController.m
//  test
//
//  Created by 谢飞 on 2017/7/20.
//  Copyright © 2017年 chen. All rights reserved.
//

#import "ViewController.h"
@interface ViewController ()

@end

@implementation ViewController

- (void)viewDidLoad {
    [super viewDidLoad];
    // Do any additional setup after loading the view, typically from a nib.
}


- (void)didReceiveMemoryWarning {
    [super didReceiveMemoryWarning];
    // Dispose of any resources that can be recreated.
}
/**
 *兼容 ios10以下 ios10采用异步跳转app
 *ios10以上应用跳转失败通过url跳转到网页
 */
-(BOOL) _jumpToApp:(NSString *) uri:(NSString *) webUrl{
    NSURL *appURL = [NSURL URLWithString:uri];
    UIApplication *application = [UIApplication sharedApplication];
    if ([application respondsToSelector:@selector(openURL:options:completionHandler:)]) {
        NSDictionary *options = @{UIApplicationOpenURLOptionUniversalLinksOnly : @YES};
        [application openURL:appURL options:options
           completionHandler:^(BOOL success) {
               NSLog(@"IOS 10 Open %@: %d",uri,success);
               if(!success){
                   NSURL *webURL = [NSURL URLWithString:webUrl];
                   [application openURL:webURL options:@{} completionHandler:nil];
               }
           }];
        return true;
    } else {
        BOOL success = [application openURL:appURL];
        NSLog(@"Below IOS 10 Open %@: %d",uri,success);
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
//-(void)viewDidAppear:(BOOL)animated{
//    [super viewDidAppear:<#animated#>];
////    self.view.backgroundColor = [UIColorgreenColor];
//    UIWindow*screenWindow = [[UIApplicationsharedApplication]keyWindow];
//}
-(UIImage *)createImageWithView:(UIView *)view{
    CGSize size = view.bounds.size;
    UIGraphicsBeginImageContextWithOptions(size, NO, [UIScreen mainScreen].scale);
    [view.layer renderInContext:UIGraphicsGetCurrentContext()];
    UIImage* image = UIGraphicsGetImageFromCurrentImageContext();
    UIGraphicsEndImageContext();
    return image;
}
- (IBAction)touchUpInside:(id)sender {
//    NSString *uri = @"fb://profile/254028725065918";
//    NSString *webUrl = @"https://www.facebook.com/254028725065918/";
//    NSURL *appURL = [NSURL URLWithString:uri];
//    UIApplication *application = [UIApplication sharedApplication];
//    if ([application respondsToSelector:@selector(openURL:options:completionHandler:)]) {
//        NSDictionary *options = @{UIApplicationOpenURLOptionUniversalLinksOnly : @YES};
//        [application openURL:appURL options:options
//           completionHandler:^(BOOL success) {
//               NSLog(@"IOS 10 Open %@: %d",uri,success);
//               if(!success){
//                   NSURL *webURL = [NSURL URLWithString:webUrl];
//                   [application openURL:webURL options:@{} completionHandler:^(BOOL success) {
//                       NSLog(@"IOS 10 Open %@: %d",webUrl,success);}
//                    ];
//               }
//           }];
//    } else {
//        BOOL success = [application openURL:appURL];
//        NSLog(@"Below IOS 10 Open %@: %d",uri,success);
//    }
    
//    [self _jumpToApp:@"fb://profile/254028725065918" :@"https://www.facebook.com/254028725065918/"];
//    UIImageView *image = [[UIImageView alloc] initWithFrame:CGRectMake(0, 0, 500, 500)];
//
//    [image setImage:
//     [self createImageWithView:
//      [[UIScreen mainScreen] snapshotViewAfterScreenUpdates:YES]]];
//    
//    [image setContentMode:UIViewContentModeScaleToFill];
//    [self.view addSubview:image];
//    [self getiOS8LastImage];
}



@end
