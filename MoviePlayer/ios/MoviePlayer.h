#if !defined(MOVIE_PLAYER_H_)
#define MOVIE_PLAYER_H_
#import "MediaPlayer/MediaPlayer.h"

enum EMoviePlayerState
{
    ECMP_STATE_STOPPED = 0,
    ECMP_STATE_PLAYING,
    ECMP_STATE_PAUSED,
    ECMP_STATE_INTERRUPTED,
    ECMP_STATE_SEEK_FORWARD,
    ECMP_STATE_SEEK_BACKWARD,
    ECMP_STATE_RESUMED,
    ECMP_STATE_FINISHED
};
@interface SkipView : UIView
{
    UIButton *m_btnSkip;
    CGFloat m_btnsTimeBeforeFade;
}
@end

@interface MoviePlayer : NSObject
+(void) play:(nullable NSString *)fileName type:(nullable NSString *)fileType isSkip:(bool)skip;
@end




#endif
