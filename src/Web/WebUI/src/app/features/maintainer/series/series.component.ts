import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { SerieDto } from '@app/core/models';
import { FaIconService, NotificationService, SerieService } from '@app/core/services';
import { ButtonComponent, TableComponent } from '@app/shared';


@Component({
    selector: 'app-series',
    imports: [
        CommonModule, ButtonComponent, TableComponent, RouterLink
    ],
    templateUrl: './series.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class SeriesComponent implements OnInit { 

  private serieService = inject(SerieService);
  private notificationService = inject(NotificationService);
  private router = inject(Router);
  private faIconService = inject(FaIconService);

  public columns :any[] = [];
  public data = signal<SerieDto[]>([]);
  public actions: any[] = [];


  public ngOnInit() {
    this.columns = [
      { name: '#', key: 'serieId' },
      { name: 'Nombre', key: 'serieName' },
      { name: 'Franquicia', key: 'franchiseName' },
      { name: 'Acciones', key: 'isActive' },
    ];

    this.actions = [
      { icon: this.faIconService.getIcon('Edit'), label: 'Editar', actionKey: 'edit', cssClass: 'bg-primary' },
      { icon: this.faIconService.getIcon('Toggle'), actionKey: 'toggle'},
    ]

    this.loadSeries();
  }


  public loadSeries() {
    this.serieService.getSeries().subscribe({
      next: (response) => {
        
        if (!response.success) {
          this.notificationService.showError('Error', response.message);
          return;
        }

        this.data.set(response.data);
      },
      error: () => {
        this.notificationService.showDefaultError();
      },
    });
  }

  public onEdit(id: string) {
    this.router.navigate(['/maintainer/series/update', id]);
  }

  public onToggle(id: string) {
    this.serieService.toggleSerie(id.toString()).subscribe({
      next: (response) => {
        if (!response.success) {
          this.notificationService.showError('Error', response.message);
          return;
        }
        this.notificationService.showSuccess('Exito', response.message);
        this.loadSeries();
      },
      error: () => {
        this.notificationService.showDefaultError();
      }
    });
  }



  public onAction(event: { id: string, actionKey: string }) {
    switch (event.actionKey) {
      case 'edit':
        this.onEdit(event.id);
        break;
      case 'toggle':
        this.onToggle(event.id);
        break;
    }
  }

}
