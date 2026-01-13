
import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { ConfigurationDto } from '@app/core/models';
import { ConfigurationService, NotificationService } from '@app/core/services';
import { ButtonComponent, TableComponent } from '@app/shared';

@Component({
    selector: 'app-configurations',
    imports: [ButtonComponent, TableComponent, RouterLink],
    templateUrl: './configurations.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ConfigurationsComponent implements OnInit {

  private router = inject(Router);
  private configurationService = inject(ConfigurationService);
  private notificationService = inject(NotificationService);

  public columns :any[] = [];
  public data = signal<ConfigurationDto[]>([]);
  public actions : any[] = [];

  public ngOnInit(): void {
    this.columns = [
      { name: '#', key: 'configurationId' },
      { name: 'Nombre', key: 'configurationName' },
      { name: 'Valor', key: 'configurationValue' },
      { name: 'Acciones', key: 'isActive' },
    ];

    this.actions = [
      { icon: 'bi bi-pencil', label: 'Editar', actionKey: 'edit', cssClass: 'bg-primary' },
      { icon: 'bi bi-toggle-on', actionKey: 'toggle'},
    ]

    this.loadConfigurations();
  }

  public loadConfigurations(): void {
    this.configurationService.getConfigurations().subscribe({
      next: (response) => {
        if (!response.success) {
          this.notificationService.showError('Error', response.message);
          return;
        }
        this.data.set(response.data);
      },
      error: () => {
        this.notificationService.showDefaultError();
      }
    });
  }

  public onEdit(id: string): void {
    this.router.navigate(['/maintainer/configurations/edit', id]);
  }

  public onToggle(id: string): void {
    this.configurationService.toggleConfiguration(id).subscribe({
      next: (response) => {
        if (!response.success) {
          this.notificationService.showError('Error', response.message);
          return;
        }
        this.notificationService.showSuccess('Exito', response.message);
        this.loadConfigurations();
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
