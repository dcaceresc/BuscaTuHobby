import { faCheck, faEdit, faPowerOff, faRotate, IconDefinition } from "@fortawesome/free-solid-svg-icons";

export interface FaIcon {
    name: string;
    icon: IconDefinition;
}

export const FA_ICONS: FaIcon[] = [
    { name: 'Edit', icon: faEdit },
    { name: 'Toggle', icon: faPowerOff },
    { name: 'Refresh', icon : faRotate },
    { name: 'Check', icon: faCheck}

];