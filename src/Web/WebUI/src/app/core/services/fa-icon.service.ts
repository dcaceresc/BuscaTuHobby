import { Injectable } from '@angular/core';
import { faExclamationTriangle, IconDefinition } from '@fortawesome/free-solid-svg-icons';
import { FA_ICONS } from '../models/fa-icon.model';

@Injectable({
  providedIn: 'root'
})
export class FaIconService {

  getIcon(iconName: string) : IconDefinition{
    const foundIcon = FA_ICONS.find(icon => icon.name === iconName);
    return foundIcon ? foundIcon.icon : faExclamationTriangle;
  }

}
