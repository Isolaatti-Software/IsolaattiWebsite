import {Route} from "@angular/router";
import {HomeComponent} from "../../home/home.component";
import {
  ProfileCreationPageComponent
} from "../../profile_creation/profile-creation-page/profile-creation-page.component";
import {FeedComponent} from "../home/feed/feed.component";
import {ProfileComponent} from "../home/profile/profile.component";

export const ROUTES: Array<Route> = [
  {
    path: '',
    component: HomeComponent,
    children: [
      {
        path: '',
        component: FeedComponent,
      },
      {
        path: 'profile',
        component: ProfileComponent
      }
    ]
  },
  {
    path: 'profile_creation',
    component: ProfileCreationPageComponent
  },
]
