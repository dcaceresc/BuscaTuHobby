import { faBars, faCheck, faEdit, faHeart, faMagnifyingGlass, faPowerOff, faRotate, faSignInAlt, faUserPlus, IconDefinition } from "@fortawesome/free-solid-svg-icons";

export interface FaIcon {
    name: string;
    icon: IconDefinition;
}

export const FA_ICONS: FaIcon[] = [
    { name: 'Edit', icon: faEdit },
    { name: 'Toggle', icon: faPowerOff },
    { name: 'Refresh', icon : faRotate },
    { name: 'Check', icon: faCheck},
    { name: 'Search', icon: faMagnifyingGlass},
    { name: 'Bars', icon: faBars },
    { name: 'Heart', icon: faHeart },
    { name: 'Logout', icon: faSignInAlt },
    { name: 'Admin', icon: faUserPlus },


];