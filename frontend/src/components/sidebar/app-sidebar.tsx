"use client";

import * as React from "react";
import {
  AudioWaveform,
  Bot,
  Trophy,
  PieChart,
  Settings2,
  PiggyBank,
  ChevronRight,
  type LucideIcon,
} from "lucide-react";

import { NavLink } from "react-router-dom";

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
  { title: "Transactions", path: "/transactions", icon: AudioWaveform },
  { title: "Receipt Scanner", path: "/scanner", icon: Bot },
  { title: "Budgets", path: "/budgets", icon: PiggyBank },
  { title: "Leaderboard", path: "/leaderboard", icon: Trophy },
  {
    title: "Settings",
    path: "/settings",
    icon: Settings2,
    subItems: [
      { title: "Profile", path: "/settings/profile" },
      { title: "Family", path: "/settings/family" },
    ],
  },
];

export function AppSidebar(props: React.ComponentProps<typeof Sidebar>) {
  const user = {
    name: "shadcn",
    email: "m@example.com",
    avatar: "/avatars/shadcn.jpg",
  };

  return (
    <Sidebar collapsible="icon" {...props}>
      <SidebarHeader>
        <SidebarGroup className="group-data-[collapsible=icon]:hidden">
          <div className="flex items-center px-1">
            <PiggyBank />
            <span className="ml-2 text-lg font-semibold">Goalbound Family</span>
          </div>
        </SidebarGroup>
      </SidebarHeader>

      <SidebarContent>
        <SidebarGroup className="group-data-[collapsible=icon]:hidden">
          <SidebarGroupLabel>Start Tracking</SidebarGroupLabel>

          <SidebarMenu>
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
