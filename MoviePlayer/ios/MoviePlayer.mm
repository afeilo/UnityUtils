#include "MoviePlayer.h"
#import <MediaPlayer/MediaPlayer.h>

class CMoviePlayer
{
public:
    static void CreateInstance(void *pCustomerData);
    static CMoviePlayer* GetInstance();
    
    CMoviePlayer(void *pCustomerData);
    bool m_isMovieNotified;
    UIView * getGlobalView();
    void addGlobalView(UIView* uiView);
    void NotifyMovie(const char *pMovieName, bool bIsFullScreen = true, bool bIsUserControllable = false);
    void PlayNotifiedMovie();
    int InitMovie(const char *pMovieName, bool bIsFullScreen = true, bool bIsUserControllable = false);
    int PlayMovie(bool hasSkip = false);
    int PauseMovie();
    int StopMovie();
    int DeinitMovie();
    bool IsMovieFinished();
    void SetAppPaused(bool paused) { m_isAppPaused = paused; }
    bool IsAppPaused() const { return m_isAppPaused; }
    ~CMoviePlayer()
    {
        DeinitMovie();
    }
    
private:
    char	m_pMovieName[128];
    bool	m_bIsFullScreen;
    bool	m_bIsUserControllable;
    void	*m_pCustomerData;
    MPMoviePlayerController	*GetMoviePlayController()const;
    MPMoviePlayerController	*m_pMovieView;
    SkipView    *m_pSkipView;
    bool    m_isAppPaused;
    bool    m_hasSkip;
    static CMoviePlayer* s_inst;
};


static void PlayMovieCallBack(CMoviePlayer *pMoviePlayer, NSNotification *aNotification)
{
    if(pMoviePlayer)
    {
        MPMoviePlayerController *pMovieController = (MPMoviePlayerController *)[aNotification object];
        NSString *pNotifyName = [aNotification name];
        
        EMoviePlayerState nState = ECMP_STATE_PLAYING;
        if([pNotifyName compare:MPMoviePlayerPlaybackDidFinishNotification] == NSOrderedSame)
        {
            nState = ECMP_STATE_FINISHED;
        }
        else if([pNotifyName compare:MPMoviePlayerPlaybackStateDidChangeNotification] == NSOrderedSame)
        {
            switch (pMovieController.playbackState) {
                case MPMoviePlaybackStateStopped:
                    nState = ECMP_STATE_STOPPED;
                    break;
                case MPMoviePlaybackStatePlaying:
                    nState = ECMP_STATE_PLAYING;
                    break;
                case MPMoviePlaybackStatePaused:
                    nState = ECMP_STATE_PAUSED;
                    break;
                case MPMoviePlaybackStateInterrupted:
                    nState = ECMP_STATE_INTERRUPTED;
                    break;
                case MPMoviePlaybackStateSeekingForward:
                    nState = ECMP_STATE_SEEK_FORWARD;
                    break;
                case MPMoviePlaybackStateSeekingBackward:
                    nState = ECMP_STATE_SEEK_BACKWARD;
                    break;
                default:
                    break;
            }
        }
        
        if(nState == ECMP_STATE_PAUSED && !pMoviePlayer->IsAppPaused())
        {
            //pMoviePlayer->PlayMovie();
        }
        else if(nState == ECMP_STATE_FINISHED || nState == ECMP_STATE_STOPPED)
        {
            pMoviePlayer->DeinitMovie();
        }
    }
}
@implementation MoviePlayer
+(void) play:(NSString *)fileName type:(nullable NSString *)fileType isSkip:(bool)isSkip{
    NSString *path = [[NSBundle mainBundle]pathForResource:fileName ofType:fileType];
    CMoviePlayer::GetInstance()->InitMovie(strdup([path UTF8String]));
    CMoviePlayer::GetInstance()->PlayMovie(isSkip);
}
@end
@interface TFFMPMoviePlayerController : MPMoviePlayerController
{
@private
    CMoviePlayer	*m_pMoviePlayer;
}
- (void) MovieCallback:(NSNotification*)aNotification;
- (void) SetMoviePlayer:(CMoviePlayer*)pMoviePlayer;
@end

@implementation TFFMPMoviePlayerController
- (void) SetMoviePlayer:(CMoviePlayer*)pMoviePlayer
{
    m_pMoviePlayer = pMoviePlayer;
}
- (void) MovieCallback:(NSNotification*)aNotification
{
    PlayMovieCallBack(m_pMoviePlayer, aNotification);
}
@end


//for skip movie button


@implementation SkipView
- (void)BtnTapped_Skip:(id)sender
{
    if (CMoviePlayer::GetInstance() != NULL)
    {
        CMoviePlayer::GetInstance()->StopMovie();
    }
}

- (id)initWithFrame:(CGRect)frame
{
    if(self = [super initWithFrame:frame])
    {
        self.backgroundColor = [UIColor clearColor];
        UIImage *image = [UIImage imageNamed:@"skip.png"];//can adapt 1x and 2x
        UIImage *strentchImage = [image stretchableImageWithLeftCapWidth:15.0 topCapHeight:0.0];
        
        m_btnSkip = [UIButton buttonWithType:UIButtonTypeRoundedRect];
        m_btnSkip.frame = CGRectMake([[UIScreen mainScreen] bounds].size.width - 75, [[UIScreen mainScreen] bounds].size.height - 45, 70, 40);//center(25, 440)
        m_btnSkip.backgroundColor = [UIColor clearColor];
        [m_btnSkip setBackgroundImage:strentchImage forState:UIControlStateNormal];
        [m_btnSkip addTarget:self action:@selector(BtnTapped_Skip:) forControlEvents:UIControlEventTouchUpInside];
        m_btnSkip.exclusiveTouch = YES;
        m_btnSkip.alpha = 1;
        [self addSubview:m_btnSkip];
    }
    return self;
}

- (void)FadebuttonOnTime:(NSTimer*)theTimer
{
    m_btnsTimeBeforeFade -= 1;
    if(m_btnsTimeBeforeFade <= 0)
    {
        m_btnSkip.alpha -= 0.05;
    }
    if(m_btnSkip.alpha <= 0)
    {
        m_btnSkip.alpha = 0;
        m_btnSkip.enabled = 0;
        [theTimer invalidate];
    }
}

- (void)dealloc
{
    m_btnSkip = nil;
}

- (void)touchesBegan:(NSSet *)touches withEvent:(UIEvent *)event
{
    if(m_btnSkip)
    {
        if (m_btnSkip.alpha == 0)
        {
            CGFloat timer_interval = 0.05;
            m_btnsTimeBeforeFade = (1 / timer_interval);
            m_btnSkip.alpha = 1;
            m_btnSkip.enabled = 1;
            
            [NSTimer scheduledTimerWithTimeInterval:timer_interval target:self selector:@selector(FadebuttonOnTime:) userInfo:nil repeats:YES];
        }
        else
        {
            m_btnSkip.alpha = 0;
            m_btnSkip.enabled = 0;
        }
    }
}
@end

MPMoviePlayerController *CMoviePlayer::GetMoviePlayController()const
{
    if(m_pMovieView != NULL)
    {
        return m_pMovieView;
    }
    
    return nil;
}

CMoviePlayer* CMoviePlayer::s_inst = NULL;

void CMoviePlayer::CreateInstance(void *pCustomerData)
{
    s_inst = new CMoviePlayer(pCustomerData);
}

CMoviePlayer* CMoviePlayer::GetInstance()
{
    if(s_inst == NULL){
        s_inst = new CMoviePlayer(NULL);
    }
    return s_inst;
}

CMoviePlayer::CMoviePlayer(void *pCustomerData)
:m_pCustomerData(pCustomerData)
,m_pMovieView(NULL)
,m_pSkipView(NULL)
,m_isMovieNotified(false)
,m_isAppPaused(false)
,m_hasSkip(false)
{
}

void CMoviePlayer::NotifyMovie(const char *pMovieName, bool bIsFullScreen, bool bIsUserControllable)
{
    m_isMovieNotified = true;
    strcpy(m_pMovieName, pMovieName);
    m_bIsFullScreen = bIsFullScreen;
    m_bIsUserControllable = bIsUserControllable;
}

void CMoviePlayer::PlayNotifiedMovie()
{
    m_isMovieNotified = false;
    InitMovie(m_pMovieName, m_bIsFullScreen, m_bIsUserControllable);
    //PlayMovie();
}



int CMoviePlayer::InitMovie(const char *pMovieName, bool bIsFullScreen, bool bIsUserControllable)
{
    DeinitMovie();
    
    NSString *tempPath = [[NSString alloc] initWithUTF8String:pMovieName];
    NSURL *url = [[NSURL alloc] initFileURLWithPath:tempPath];
    BOOL bIsNew = NO;
    
    MPMoviePlayerController *moviePlayerController = nil;
    TFFMPMoviePlayerController *movieViewPlayerController = nil;
    if(m_pMovieView == NULL)
    {
        m_pMovieView = movieViewPlayerController = [[TFFMPMoviePlayerController alloc] initWithContentURL:url];
        [movieViewPlayerController SetMoviePlayer:this];
        
        bIsNew = YES;
    }
    
    moviePlayerController = (MPMoviePlayerController*)GetMoviePlayController();
    moviePlayerController.controlStyle = bIsUserControllable?MPMovieControlStyleFullscreen:MPMovieControlStyleNone;
    movieViewPlayerController = (TFFMPMoviePlayerController*)m_pMovieView;
    moviePlayerController.fullscreen = bIsFullScreen?YES:NO;
    
    if(bIsNew)
    {
        moviePlayerController.scalingMode = MPMovieScalingModeAspectFit;
        
        [[NSNotificationCenter defaultCenter]	addObserver:(id)m_pMovieView
                                                 selector:@selector(MovieCallback:)
                                                     name:MPMoviePlayerPlaybackDidFinishNotification
                                                   object:moviePlayerController];
        
        [[NSNotificationCenter defaultCenter] addObserver:(id)m_pMovieView
                                                 selector:@selector(MovieCallback:)
                                                     name:MPMoviePlayerPlaybackStateDidChangeNotification
                                                   object:moviePlayerController];
        
        if ( !bIsUserControllable )
        {
            [movieViewPlayerController.view setUserInteractionEnabled:NO];
        }
        //        #ifdef DEFAULT_PORTRAINT_MODE
        movieViewPlayerController.view.bounds = CGRectMake(0, 0, [[UIScreen mainScreen] bounds].size.width, [[UIScreen mainScreen] bounds].size.height);
        movieViewPlayerController.view.center = CGPointMake([[UIScreen mainScreen] bounds].size.width / 2, [[UIScreen mainScreen] bounds].size.height / 2);
        UIView * globalView = UnityGetGLView();
        [globalView addSubview:movieViewPlayerController.view];
        [globalView bringSubviewToFront:movieViewPlayerController.view];
    }
    else
    {
        [moviePlayerController setContentURL:url];
        [moviePlayerController play];
    }
    
    return 0;
}

int CMoviePlayer::PlayMovie(bool hasSkip)
{
    if(m_pMovieView != nil)
    {
        [(MPMoviePlayerController*)GetMoviePlayController() play];
        
        if ( !m_hasSkip && hasSkip )
        {
            m_hasSkip = true;
            m_pSkipView = [[SkipView alloc]initWithFrame:CGRectMake(0, 0, [[UIScreen mainScreen] bounds].size.width, [[UIScreen mainScreen] bounds].size.height)];
            UIView * globalView = UnityGetGLView();
            [globalView addSubview:(id)m_pSkipView];
            [globalView bringSubviewToFront:(id)m_pSkipView];
        }
    }
    
    return 0;
}

int CMoviePlayer::PauseMovie()
{
    if(m_pMovieView != nil)
    {
        [(MPMoviePlayerController*)GetMoviePlayController() pause];
    }
    
    return 0;
}

int CMoviePlayer::StopMovie()
{
    if(m_pMovieView != nil)
    {
        [(MPMoviePlayerController*)GetMoviePlayController() stop];
    }
    
    return 0;
}

int CMoviePlayer::DeinitMovie()
{
    if(m_pMovieView != nil)
    {
        MPMoviePlayerController *moviePlayerController = (MPMoviePlayerController*)GetMoviePlayController();
        
        [[NSNotificationCenter defaultCenter]	removeObserver:(id)m_pMovieView
                                                        name:nil
                                                      object:moviePlayerController];
        
        [moviePlayerController stop];
        
        [((TFFMPMoviePlayerController *)m_pMovieView).view removeFromSuperview];
        m_pMovieView = nil;
        
        if ( m_hasSkip )
        {
            m_hasSkip = false;
            [(id)m_pSkipView removeFromSuperview];
            m_pSkipView = nil;
        }
    
    }
    
    return 0;
}

bool CMoviePlayer::IsMovieFinished()
{
    return (m_pMovieView == nil);
}



