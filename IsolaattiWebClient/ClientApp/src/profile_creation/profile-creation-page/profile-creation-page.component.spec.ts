import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProfileCreationPageComponent } from './profile-creation-page.component';

describe('ProfileCreationPageComponent', () => {
  let component: ProfileCreationPageComponent;
  let fixture: ComponentFixture<ProfileCreationPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ProfileCreationPageComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ProfileCreationPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
