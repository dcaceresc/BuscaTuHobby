import { TestBed } from '@angular/core/testing';

import { UniversesService } from './universes.service';

describe('UniversesService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: UniversesService = TestBed.get(UniversesService);
    expect(service).toBeTruthy();
  });
});
