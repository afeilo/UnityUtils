//
//  AssetManager.m
//  test
//
//  Created by 谢飞 on 2017/11/3.
//  Copyright © 2017年 chen. All rights reserved.
//

#import "AssetManager.h"
@implementation PHAsset (WJ)
+ (PHAsset *)latestAsset {
    // 获取所有资源的集合，并按资源的创建时间排序
    PHFetchOptions *options = [[PHFetchOptions alloc] init];
    options.sortDescriptors = @[[NSSortDescriptor sortDescriptorWithKey:@"creationDate" ascending:NO]];
    PHFetchResult *assetsFetchResults = [PHAsset fetchAssetsWithOptions:options];
    return [assetsFetchResults firstObject];
}
@end

