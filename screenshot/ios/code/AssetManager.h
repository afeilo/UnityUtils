//
//  AssetManager.h
//  test
//
//  Created by 谢飞 on 2017/11/3.
//  Copyright © 2017年 chen. All rights reserved.
//

#ifndef AssetManager_h
#define AssetManager_h
#import <Photos/Photos.h>
@interface PHAsset (WJ)
/**
 *  获取最新一张图片
 */
+ (PHAsset *)latestAsset;
@end

#endif /* AssetManager_h */
