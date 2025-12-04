import { AppSidebar } from "@/components/sidebar/app-sidebar";
import { SidebarProvider, SidebarInset, SidebarTrigger } from "@/components/ui/sidebar";
import { Separator } from "@/components/ui/separator";

interface LayoutProps {
  children: React.ReactNode;
}

export const Layout: React.FC<LayoutProps> = ({ children }) => {
  return (
    <SidebarProvider>
      <AppSidebar />

      {/* Everything on the right side */}
      <SidebarInset className="flex flex-col h-screen">
        {/* Mobile header with menu trigger */}
        <header className="flex h-16 shrink-0 items-center gap-2 border-b px-4 md:hidden">
          <SidebarTrigger className="-ml-1" />
          <Separator orientation="vertical" className="mr-2 h-4" />
          <div className="flex items-center gap-2">
            <span className="font-bold text-lg bg-gradient-to-r from-purple-600 to-purple-800 bg-clip-text text-transparent">
              Goalbound Family
            </span>
          </div>
        </header>

        {/* Main page content */}
        <main className="flex-1 p-4 overflow-auto">{children}</main>
      </SidebarInset>
    </SidebarProvider>
  );
};
