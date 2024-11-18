export interface DashboardMenuDto{
    menuName: string;
    menuOrder: number;
    subMenus: DashboardSubMenuDto[];
}

export interface DashboardSubMenuDto{
    subMenuName: string;
    subMenuSlug: string;
}