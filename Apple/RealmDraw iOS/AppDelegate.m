////////////////////////////////////////////////////////////////////////////
//
// Copyright 2016 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
////////////////////////////////////////////////////////////////////////////

#import "AppDelegate.h"
#import <Realm/Realm.h>
#import "DrawView.h"
#import "Constants.h"

@import RealmLoginKit;

@interface AppDelegate ()
@property (nonatomic, strong) UIActivityIndicatorView *activityIndicatorView;
@end

@implementation AppDelegate

- (BOOL)application:(UIApplication *)application didFinishLaunchingWithOptions:(NSDictionary *)launchOptions
{
    application.applicationSupportsShakeToEdit = YES;

    self.window = [[UIWindow alloc] initWithFrame:[[UIScreen mainScreen] bounds]];
    self.window.rootViewController = [[UIViewController alloc] init];
    self.window.rootViewController.view.backgroundColor = [UIColor whiteColor];

    // Setup Global Error Handler
    [RLMSyncManager sharedManager].errorHandler = ^(NSError *error, RLMSyncSession *session) {
        NSLog(@"A global error has occurred! %@", error);
    };
    
    [self.window makeKeyAndVisible];
    
    RLMLoginViewController *loginController = [[RLMLoginViewController alloc] initWithStyle:LoginViewControllerStyleLightTranslucent];
    loginController.serverURL = kIPAddress;
    [self.window.rootViewController presentViewController:loginController animated:NO completion:nil];
    
    __weak typeof(loginController) weakController = loginController;
    loginController.logInSuccessfulHandler = ^(RLMSyncUser *user) {
        // Logged in setup the default Realm
        // The Realm virtual path on the server.
        // The `~` represents the Realm user ID. Since the user ID is not known until you
        // log in, the ~ is used as short-hand to represent this.
        NSURL *syncURL = [NSURL URLWithString:[NSString stringWithFormat:@"realm://%@:9080/~/Draw", weakController.serverURL]];
        RLMSyncConfiguration *syncConfig = [[RLMSyncConfiguration alloc] initWithUser:user realmURL:syncURL];
        RLMRealmConfiguration *defaultConfig = [RLMRealmConfiguration defaultConfiguration];
        defaultConfig.syncConfiguration = syncConfig;
        [RLMRealmConfiguration setDefaultConfiguration:defaultConfig];
        
        DrawView *drawView = [[DrawView alloc] initWithFrame:self.window.rootViewController.view.bounds];
        [self.window.rootViewController.view addSubview:drawView];

        [self.window.rootViewController dismissViewControllerAnimated:YES completion:nil];
    };
    
    return YES;
}

@end
