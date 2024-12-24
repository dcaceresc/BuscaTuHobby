export interface DashboardMenuDto{
    menuName: string;
    menuOrder: number;
    menuSlug: string;
    subMenus: DashboardSubMenuDto[];
}

export interface DashboardSubMenuDto{
    subMenuName: string;
    subMenuOrder: number;
    subMenuSlug: string;
}