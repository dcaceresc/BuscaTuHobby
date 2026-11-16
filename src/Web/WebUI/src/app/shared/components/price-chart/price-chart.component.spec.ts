import { registerLocaleData } from '@angular/common';
import localeEsCl from '@angular/common/locales/es-CL';
import { TestBed } from '@angular/core/testing';
import { beforeEach, describe, expect, it } from 'vitest';
import { PriceChartComponent } from './price-chart.component';
import { PricePointDto } from './price-point.model';

registerLocaleData(localeEsCl, 'es-CL');

const THREE_POINTS: PricePointDto[] = [
  { date: '2025-09-01', minPrice: 12990, maxPrice: 15990, offerCount: 2 },
  { date: '2025-09-15', minPrice: 11990, maxPrice: 14990, offerCount: 3 },
  { date: '2025-09-30', minPrice: 13990, maxPrice: 17990, offerCount: 4 }
];

describe('PriceChartComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [PriceChartComponent] }).compileComponents();
  });

  function render(points: PricePointDto[]) {
    const fixture = TestBed.createComponent(PriceChartComponent);
    fixture.componentRef.setInput('points', points);
    fixture.detectChanges();
    return fixture;
  }

  function coordinatePairs(element: SVGElement): string[] {
    return (element.getAttribute('points') ?? '').trim().split(/\s+/);
  }

  function yValues(pairs: string[]): number[] {
    return pairs.map((pair) => Number(pair.split(',')[1]));
  }

  function xValues(pairs: string[]): string[] {
    return pairs.map((pair) => pair.split(',')[0]);
  }

  it('shows the empty state and no chart when there are no points', () => {
    const fixture = render([]);
    const host: HTMLElement = fixture.nativeElement;

    expect(host.textContent).toContain('Todavía no hay precios registrados para este producto.');
    expect(host.querySelector('svg')).toBeNull();
  });

  it('shows the single-point state without any line when there is one point', () => {
    const fixture = render([{ date: '2025-09-01', minPrice: 12990, maxPrice: 12990, offerCount: 1 }]);
    const host: HTMLElement = fixture.nativeElement;

    expect(host.textContent).toContain('12.990');
    expect(host.textContent).toContain('Recopilando');
    expect(host.querySelector('polyline')).toBeNull();
    expect(host.querySelector('svg')).toBeNull();
  });

  it('renders one coordinate pair per point in the minimum-price polyline', () => {
    const fixture = render(THREE_POINTS);
    const polyline = fixture.nativeElement.querySelector('polyline') as SVGPolylineElement | null;

    expect(polyline).not.toBeNull();
    const pairs = coordinatePairs(polyline!);
    expect(pairs).toHaveLength(3);
    for (const pair of pairs) {
      expect(pair).toMatch(/^-?\d+(\.\d+)?,-?\d+(\.\d+)?$/);
    }
  });

  it('lists every point in the accessible table inside a visually-hidden wrapper', () => {
    const fixture = render(THREE_POINTS);
    const host: HTMLElement = fixture.nativeElement;

    const wrapper = host.querySelector('.visually-hidden') as HTMLElement | null;
    expect(wrapper).not.toBeNull();

    const table = wrapper!.querySelector('table') as HTMLTableElement | null;
    expect(table).not.toBeNull();
    expect(table!.querySelector('caption')?.textContent?.trim()).toBeTruthy();
    expect(table!.querySelectorAll('thead tr')).toHaveLength(1);
    expect(table!.querySelectorAll('tbody tr')).toHaveLength(3);
    expect(table!.querySelectorAll('tr')).toHaveLength(4);

    const rows = Array.from(table!.querySelectorAll('tbody tr')).map((row) =>
      Array.from(row.querySelectorAll('td')).map((cell) => cell.textContent?.trim() ?? '')
    );
    expect(rows.every((cells) => cells.length === 4)).toBe(true);
    expect(rows[0][0]).toContain('2025');
    expect(rows[0][1]).toContain('12.990');
    expect(rows[0][2]).toContain('15.990');
    expect(rows[0][3]).toBe('2');
  });

  it('draws the observed maximum as a second polyline over the same x positions', () => {
    const fixture = render(THREE_POINTS);
    const host: HTMLElement = fixture.nativeElement;
    const polylines = Array.from(host.querySelectorAll('polyline')) as SVGPolylineElement[];

    expect(polylines).toHaveLength(2);
    const minPairs = coordinatePairs(polylines[0]);
    const maxPairs = coordinatePairs(polylines[1]);
    expect(minPairs).toHaveLength(3);
    expect(maxPairs).toHaveLength(3);
    expect(xValues(maxPairs)).toEqual(xValues(minPairs));

    // En SVG el eje y crece hacia abajo, así que el máximo de cada día queda sobre el mínimo.
    yValues(maxPairs).forEach((maxY, index) => {
      expect(Number.isFinite(maxY)).toBe(true);
      expect(maxY).toBeLessThan(yValues(minPairs)[index]);
    });
  });

  it('fills the band between the minimum and the maximum inside the observed values', () => {
    const fixture = render(THREE_POINTS);
    const host: HTMLElement = fixture.nativeElement;
    const band = host.querySelector('polygon.price-chart__band') as SVGPolygonElement | null;

    expect(band).not.toBeNull();
    const bandPairs = coordinatePairs(band!);
    // La banda recorre el mínimo hacia adelante y el máximo hacia atrás: dos vértices por día.
    expect(bandPairs).toHaveLength(THREE_POINTS.length * 2);

    const polylines = Array.from(host.querySelectorAll('polyline')) as SVGPolylineElement[];
    const minPairs = coordinatePairs(polylines[0]);
    const maxPairs = coordinatePairs(polylines[1]);
    const observedYs = new Set([...yValues(minPairs), ...yValues(maxPairs)]);
    const bandYs = yValues(bandPairs);

    bandYs.forEach((y) => expect(observedYs.has(y)).toBe(true));
    expect(Math.max(...bandYs)).toBe(Math.max(...yValues(minPairs)));
    expect(Math.min(...bandYs)).toBe(Math.min(...yValues(maxPairs)));
    expect(new Set(xValues(bandPairs))).toEqual(new Set(xValues(minPairs)));
  });

  it('no longer draws the maximum as a flat dashed reference line', () => {
    const fixture = render(THREE_POINTS);
    const host: HTMLElement = fixture.nativeElement;

    expect(host.querySelector('.price-chart__max-line')).toBeNull();
    expect(host.textContent).toContain('Máx.');
    expect(host.textContent).toContain('17.990');
  });

  it('exposes an accessible description with the price range and the period', () => {
    const fixture = render(THREE_POINTS);
    const svg = fixture.nativeElement.querySelector('svg') as SVGSVGElement | null;

    expect(svg).not.toBeNull();
    expect(svg!.getAttribute('role')).toBe('img');
    const label = svg!.getAttribute('aria-label')?.trim() ?? '';
    expect(label.length).toBeGreaterThan(0);
    expect(label).toContain('11.990');
    expect(label).toContain('17.990');
  });

  it('renders a flat polyline when every price is equal', () => {
    const fixture = render([
      { date: '2025-09-01', minPrice: 9900, maxPrice: 9900, offerCount: 1 },
      { date: '2025-09-02', minPrice: 9900, maxPrice: 9900, offerCount: 1 }
    ]);
    const host: HTMLElement = fixture.nativeElement;
    const polylines = Array.from(host.querySelectorAll('polyline')) as SVGPolylineElement[];

    expect(polylines).toHaveLength(2);
    const minPairs = coordinatePairs(polylines[0]);
    expect(minPairs).toHaveLength(2);
    const ys = yValues(minPairs);
    expect(ys.every((y) => Number.isFinite(y))).toBe(true);
    expect(ys[0]).toBe(ys[1]);

    // Con precios iguales la banda se aplana: ambos extremos comparten la misma y.
    expect(yValues(coordinatePairs(polylines[1]))).toEqual(ys);
    expect(host.querySelector('polygon.price-chart__band')).not.toBeNull();
  });
});
