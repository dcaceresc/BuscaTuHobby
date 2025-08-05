import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { AuthService } from '@app/core/services';

@Component({
    selector: 'app-confirm-email',
    imports: [
        CommonModule,
    ],
    templateUrl: './confirm-email.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ConfirmEmailComponent implements OnInit {

  private route = inject(ActivatedRoute);
  private authService = inject(AuthService);

  public userId! : string | null;
  public token!: string | null;
  public header = signal<string>("");
  public title = signal<string>("");
  public message = signal<string>("");
  public svgIcon = signal<boolean>(false);

  public ngOnInit(): void {

    this.userId = this.route.snapshot.paramMap.get('userId');
    this.token = this.route.snapshot.paramMap.get('token');
    
    if(this.userId && this.token){
      this.authService.confirmEmail({userId: this.userId, token: this.token}).subscribe({
        next : (response) => {

          this.header.set(response.success ? "Confirmación de Correo Exitosa" : "Confirmación de Correo Fallida");
          this.title.set(response.success ? "¡Felicidades!" : "¡Lo sentimos!");
          this.svgIcon.set(response.success);
          this.message.set(response.message);
        },
        error : () => {
          this.header.set("Error de Confirmación");
          this.title.set("¡Lo sentimos!");
          this.message.set("Ah ocurrido un error al confirmar el email");
        }
      });
    }else{
      this.header.set("Error de Confirmación");
      this.title.set("¡Lo sentimos!");
      this.message.set("No se ha podido confirmar el email");
    }
    
  }

}
