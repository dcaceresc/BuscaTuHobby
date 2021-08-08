import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ModalUniverseComponent } from './modal-universe.component';

describe('ModalUniverseComponent', () => {
  let component: ModalUniverseComponent;
  let fixture: ComponentFixture<ModalUniverseComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ModalUniverseComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ModalUniverseComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
