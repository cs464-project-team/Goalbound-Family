"use client";

import * as React from "react";
import {
  Bot,
  Trophy,
  PieChart,
  Settings2,
  PiggyBank,
  ChevronRight,
  Receipt,
  type LucideIcon,
} from "lucide-react";

import { NavLink } from "react-router-dom";
import { useAuthContext } from "@/context/AuthProvider";

import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from "@/components/ui/collapsible";

import { NavUser } from "@/components/sidebar/nav-user";
import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarHeader,
  SidebarRail,
  SidebarGroup,
  SidebarGroupLabel,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarMenuSub,
  SidebarMenuSubButton,
  SidebarMenuSubItem,
} from "@/components/ui/sidebar";

interface RouteItem {
  title: string;
  path: string;
  icon?: LucideIcon;
  subItems?: RouteItem[];
}

// Define routes for your financial tracking app
const routes: RouteItem[] = [
  { title: "Dashboard", path: "/dashboard", icon: PieChart },
  { title: "Expenses", path: "/expenses", icon: Receipt },
  { title: "Receipt Scanner", path: "/scanner", icon: Bot },
  { title: "Budgets", path: "/budgets", icon: PiggyBank },
  { title: "Leaderboard", path: "/leaderboard", icon: Trophy },
  {
    title: "Settings",
    path: "/settings",
    icon: Settings2,
    subItems: [
      { title: "Profile", path: "/settings/profile" },
      { title: "Households", path: "/settings/family" },
    ],
  },
];

export function AppSidebar(props: React.ComponentProps<typeof Sidebar>) {
  const { session } = useAuthContext();

  const userEmail = session?.user?.email || "user@example.com";
  const displayName = userEmail.split('@')[0];

  const user = {
    name: displayName,
    email: userEmail,
    avatar: "",
  };

  return (
    <Sidebar collapsible="icon" {...props}>
      <SidebarHeader className="bg-gradient-to-br from-purple-600 to-purple-800 border-b border-purple-700">
        <SidebarGroup className="group-data-[collapsible=icon]:hidden">
          <div className="flex items-center px-1 py-2">
            <div className="flex items-center justify-center w-10 h-10 bg-white/20 rounded-lg backdrop-blur-sm">
              <PiggyBank className="text-white" size={24} />
            </div>
            <span className="ml-3 text-xl font-bold text-white tracking-tight">Goalbound Family</span>
          </div>
        </SidebarGroup>
      </SidebarHeader>

      <SidebarContent className="px-2">
        <SidebarGroup className="group-data-[collapsible=icon]:hidden mt-4">
          <SidebarGroupLabel className="px-2 mb-2">Start Tracking</SidebarGroupLabel>

          <SidebarMenu className="gap-1">
            {routes.map((route) => {
              if (route.subItems) {
                return (
                  <Collapsible key={route.path} asChild defaultOpen={false}>
                    <SidebarMenuItem>
                      <CollapsibleTrigger asChild>
                        <SidebarMenuButton tooltip={route.title}>
                          {route.icon && <route.icon />}
                          <span>{route.title}</span>
                          <ChevronRight className="ml-auto transition-transform duration-200 group-data-[state=open]/collapsible:rotate-90" />
                        </SidebarMenuButton>
                      </CollapsibleTrigger>
                      <CollapsibleContent>
                        <SidebarMenuSub>
                          {route.subItems.map((sub) => (
                            <SidebarMenuSubItem key={sub.path}>
                              <SidebarMenuSubButton asChild>
                                <NavLink to={sub.path}>
                                  <span>{sub.title}</span>
                                </NavLink>
                              </SidebarMenuSubButton>
                            </SidebarMenuSubItem>
                          ))}
                        </SidebarMenuSub>
                      </CollapsibleContent>
                    </SidebarMenuItem>
                  </Collapsible>
                );
              }

              return (
                <SidebarMenuItem key={route.path}>
                  <SidebarMenuButton asChild>
                    <NavLink to={route.path}>
                      {route.icon && <route.icon />}
                      <span>{route.title}</span>
                    </NavLink>
                  </SidebarMenuButton>
                </SidebarMenuItem>
              );
            })}
          </SidebarMenu>
        </SidebarGroup>
      </SidebarContent>

      <SidebarFooter>
        <NavUser user={user} />
      </SidebarFooter>

      <SidebarRail />
    </Sidebar>
  );
}
