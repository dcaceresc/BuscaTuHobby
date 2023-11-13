import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { inventoryDto } from 'src/app/core/models/inventory.model';
import { faEdit, faPowerOff } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { RouterLink } from '@angular/router';
import { InventoriesService } from 'src/app/core/services/inventories.service';

@Component({
  standalone: true,
  imports: [CommonModule,RouterLink,FontAwesomeModule],
  templateUrl: './list-inventories.component.html',
  styleUrls: ['./list-inventories.component.scss']
})
export class ListInventoriesComponent {
  inventories = signal<inventoryDto[]>([]);
  faPowerOff = faPowerOff;
  faEdit = faEdit;
  currentPage = signal(1);
  itemsPerPage = 10;

  constructor(private inventoriesService:InventoriesService) {}

  ngOnInit(): void {
    this.inventoriesService.GetAll().subscribe(items => this.inventories.set(items));
  }

  getPaginatedData(){
    const startIndex = (this.currentPage() - 1) * this.itemsPerPage;
    const endIndex = startIndex + this.itemsPerPage;
    return this.inventories()?.slice(startIndex,endIndex);
  }

  setPage(pageNumber: number) {
    this.currentPage.set(pageNumber);
  }

  range(start: number) {
    const result = [];
    for (let i = start; i <= this.nPage(); i++) {
      result.push(i);
    }
    return result;
  }

  nPage(){
    return Math.ceil(this.inventories()?.length / this.itemsPerPage)
  }

  toggle(id:number){
    const inventory = this.inventories().find(x => x.id === id);

    if(inventory){
      this.inventoriesService.Toggle(id).subscribe(
        () => {
          this.inventoriesService.GetAll().subscribe(items => this.inventories.set(items));
        }
      );
    }
  }
}
