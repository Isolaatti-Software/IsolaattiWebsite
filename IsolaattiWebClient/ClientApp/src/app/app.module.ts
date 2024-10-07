import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';

import { AppComponent } from './app.component';
import { ProfileCreationPageComponent } from '../profile_creation/profile-creation-page/profile-creation-page.component';
import {RouterModule, RouterOutlet} from "@angular/router";
import { HomeComponent } from '../home/home.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import {ROUTES} from "./routing/routes";
import {MatSidenavModule} from "@angular/material/sidenav";
import {MatButtonModule} from "@angular/material/button";
import {MatListModule} from "@angular/material/list";
import {MatIconModule} from "@angular/material/icon";
import { FeedComponent } from './home/feed/feed.component';
import { ProfileComponent } from './home/profile/profile.component';
import {MatTabsModule} from "@angular/material/tabs";
import { NewPostDialogComponent } from './posting/new-post-dialog/new-post-dialog.component';
import {MatMenuModule} from "@angular/material/menu";
import {MAT_DIALOG_DEFAULT_OPTIONS, MatDialog, MatDialogModule, MatDialogRef} from "@angular/material/dialog";
import {MatToolbarModule} from "@angular/material/toolbar";
import { PostComponent } from './posting/post/post.component';
import {MatCardModule} from "@angular/material/card";
import { PostListComponent } from './posting/post-list/post-list.component';

@NgModule({
  declarations: [
    AppComponent,
    ProfileCreationPageComponent,
    HomeComponent,
    FeedComponent,
    ProfileComponent,
    NewPostDialogComponent,
    PostComponent,
    PostListComponent,
  ],
  imports: [
    BrowserModule.withServerTransition({appId: 'ng-cli-universal'}),
    HttpClientModule,
    FormsModule,
    RouterModule.forRoot(ROUTES),
    BrowserAnimationsModule,
    MatSidenavModule,
    MatButtonModule,
    MatListModule,
    MatIconModule,
    MatTabsModule,
    MatMenuModule,
    MatDialogModule,
    MatToolbarModule,
    MatCardModule
  ],
  providers: [
    {provide: MAT_DIALOG_DEFAULT_OPTIONS, useValue: {hasBackdrop: false}}
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
