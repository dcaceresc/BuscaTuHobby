import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { GunplaComponent } from './gunpla.component';

describe('GunplaComponent', () => {
  let component: GunplaComponent;
  let fixture: ComponentFixture<GunplaComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ GunplaComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(GunplaComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
