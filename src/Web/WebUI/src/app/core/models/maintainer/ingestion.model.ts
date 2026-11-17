import { formatObservedAt } from '../dashboard/dashboard.model';

// El estado de una ejecución viaja como entero desde la API y su orden es el del enum ImportRunStatus del dominio:
// 0 Running, 1 Completed, 2 Failed. Un valor desconocido se muestra sin etiqueta inventada.
const IMPORT_RUN_STATUS_LABELS: Record<number, string> = {
    0: 'En ejecución',
    1: 'Completada',
    2: 'Fallida'
};

/** Traduce el estado numérico de una ejecución a su etiqueta en español. */
export function importRunStatusLabel(status: number): string {
    return IMPORT_RUN_STATUS_LABELS[status] ?? 'Sin dato';
}

/** Una ejecución abierta (Running) es la que impide disparar otra de la misma tienda. */
export function isImportRunOpen(run: IngestionRunDto | null): boolean {
    return run !== null && run.status === 0;
}

/** Las fechas de una ejecución llegan sin zona horaria igual que las de una oferta: se formatean con el mismo criterio. */
export function formatIngestionDate(value: string | null): string {
    return value ? formatObservedAt(value) : 'Sin dato';
}

export interface IngestionRunDto {
    importRunId: string;
    startedAtUtc: string;
    finishedAtUtc: string | null;
    status: number;
    requestBudget: number;
    discoveredCount: number;
    storedCount: number;
    skippedCount: number;
    failedCount: number;
}

export interface IngestionStatusDto {
    storeId: string;
    storeName: string;
    storeSlug: string;
    // Nulo sólo si la fila perdió el conector: el estado lista las tiendas configuradas, pero el contrato lo admite.
    connectorKey: string | null;
    isActive: boolean;
    totalProducts: number;
    coveredProducts: number;
    coveragePercentage: number;
    offersObservedLast24Hours: number;
    offersOlderThanSevenDays: number;
    lastRun: IngestionRunDto | null;
}

export interface StartIngestionRun {
    storeSlug: string;
    // Opcional: vacío significa usar el presupuesto configurado en la tienda. La API lo recorta, nunca lo amplía.
    requestBudget?: number;
}
