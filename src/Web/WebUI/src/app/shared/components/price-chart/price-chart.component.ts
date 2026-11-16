import { DatePipe, DecimalPipe, formatDate, formatNumber, registerLocaleData } from '@angular/common';
import localeEsCl from '@angular/common/locales/es-CL';
import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { PricePointDto } from './price-point.model';

// Los precios y las fechas se formatean en locale chileno; registrarlo aquí mantiene los datos de locale en el
// chunk diferido del componente en lugar del bundle inicial.
registerLocaleData(localeEsCl, 'es-CL');

const CHART_WIDTH = 320;
const CHART_HEIGHT = 130;
const CHART_PADDING = { top: 14, right: 10, bottom: 24, left: 50 };
const LOCALE = 'es-CL';
const UTC = 'UTC';
const PRICE_FORMAT = '1.0-0';
const TABLE_DATE_FORMAT = 'd MMM y';

const round2 = (value: number): number => Math.round(value * 100) / 100;

export interface PriceChartGeometryPoint {
  date: string;
  minPrice: number;
  offerCount: number;
  x: number;
  y: number;
}

export interface PriceChartTick {
  value: number;
  x1: number;
  x2: number;
  y: number;
}

export interface PriceChartGeometry {
  viewBox: string;
  points: PriceChartGeometryPoint[];
  polyline: string;
  area: string;
  firstX: number;
  lastX: number;
  baselineY: number;
  dateLabelY: number;
  minPrice: number;
  maxPrice: number;
  maxPriceY: number;
  minPoint: PriceChartGeometryPoint | null;
  minLabelX: number;
  ticks: PriceChartTick[];
}

function buildGeometry(series: PricePointDto[]): PriceChartGeometry {
  const innerWidth = CHART_WIDTH - CHART_PADDING.left - CHART_PADDING.right;
  const innerHeight = CHART_HEIGHT - CHART_PADDING.top - CHART_PADDING.bottom;
  const baselineY = round2(CHART_PADDING.top + innerHeight);
  const minPrice = series.length > 0 ? Math.min(...series.map((point) => point.minPrice)) : 0;
  const maxPrice = series.length > 0 ? Math.max(...series.map((point) => point.maxPrice)) : 0;

  // Una sola fecha o una serie de precios iguales no tiene extensión vertical: se amplía la escala en
  // un peso para que la interpolación nunca divida por cero y una línea plana quede centrada.
  const hasSpan = maxPrice > minPrice;
  const floor = hasSpan ? minPrice : minPrice - 1;
  const ceiling = hasSpan ? maxPrice : maxPrice + 1;
  const yFor = (price: number): number =>
    round2(CHART_PADDING.top + (1 - (price - floor) / (ceiling - floor)) * innerHeight);

  const stepX = series.length > 1 ? innerWidth / (series.length - 1) : 0;
  const points: PriceChartGeometryPoint[] = series.map((point, index) => ({
    date: point.date,
    minPrice: point.minPrice,
    offerCount: point.offerCount,
    x: round2(
      series.length > 1 ? CHART_PADDING.left + index * stepX : CHART_PADDING.left + innerWidth / 2
    ),
    y: yFor(point.minPrice)
  }));

  const minPoint = points.reduce<PriceChartGeometryPoint | null>(
    (current, point) => (current === null || point.minPrice < current.minPrice ? point : current),
    null
  );
  const firstX = points[0]?.x ?? CHART_PADDING.left;
  const lastX = points[points.length - 1]?.x ?? CHART_PADDING.left + innerWidth;
  const axisEndX = round2(CHART_PADDING.left + innerWidth);

  const midValue = Math.round((floor + ceiling) / 2);
  const ticks: PriceChartTick[] = [
    { value: Math.round(ceiling), x1: CHART_PADDING.left, x2: axisEndX, y: yFor(ceiling) },
    { value: midValue, x1: CHART_PADDING.left, x2: axisEndX, y: yFor(midValue) },
    { value: Math.round(floor), x1: CHART_PADDING.left, x2: axisEndX, y: yFor(floor) }
  ];

  return {
    viewBox: `0 0 ${CHART_WIDTH} ${CHART_HEIGHT}`,
    points,
    polyline: points.map((point) => `${point.x},${point.y}`).join(' '),
    area: [
      `${firstX},${baselineY}`,
      ...points.map((point) => `${point.x},${point.y}`),
      `${lastX},${baselineY}`
    ].join(' '),
    firstX,
    lastX,
    baselineY,
    dateLabelY: CHART_HEIGHT - 6,
    minPrice,
    maxPrice,
    maxPriceY: yFor(maxPrice),
    minPoint,
    minLabelX: minPoint
      ? round2(
          Math.min(
            Math.max(minPoint.x, CHART_PADDING.left + 20),
            CHART_WIDTH - CHART_PADDING.right - 20
          )
        )
      : 0,
    ticks
  };
}

@Component({
  selector: 'app-price-chart',
  imports: [DatePipe, DecimalPipe],
  templateUrl: './price-chart.component.html',
  styleUrl: './price-chart.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PriceChartComponent {
  readonly points = input.required<PricePointDto[]>();
  readonly label = input<string>('Evolución de precios');

  /** Copia cronológica de la entrada; el padre es dueño del arreglo y no debe mutarse. */
  readonly series = computed(() =>
    [...this.points()].sort((a, b) => a.date.localeCompare(b.date))
  );

  readonly geometry = computed(() => buildGeometry(this.series()));

  readonly ariaLabel = computed(() => {
    const series = this.series();
    if (series.length < 2) {
      return '';
    }

    const from = formatDate(series[0].date, TABLE_DATE_FORMAT, LOCALE, UTC);
    const to = formatDate(series[series.length - 1].date, TABLE_DATE_FORMAT, LOCALE, UTC);
    const minimum = formatNumber(this.geometry().minPrice, LOCALE, PRICE_FORMAT);
    const maximum = formatNumber(this.geometry().maxPrice, LOCALE, PRICE_FORMAT);

    return `Evolución del precio mínimo en pesos chilenos entre el ${from} y el ${to}: de $${minimum} a $${maximum}.`;
  });
}
