import { TestBed } from '@angular/core/testing';

import { GunplasService } from './gunplas.service';

describe('GunplasService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: GunplasService = TestBed.get(GunplasService);
    expect(service).toBeTruthy();
  });
});
