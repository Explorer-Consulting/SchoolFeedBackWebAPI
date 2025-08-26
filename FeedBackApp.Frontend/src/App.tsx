import { Toaster } from "@/components/ui/toaster";
import { Toaster as Sonner } from "@/components/ui/sonner";
import { TooltipProvider } from "@/components/ui/tooltip";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { BrowserRouter, Routes, Route } from "react-router-dom";
import { GoogleOAuthProvider } from "@react-oauth/google"; 

import Index from "./pages/Index";
import NotFound from "./pages/NotFound";
import NoAccess from "./pages/NoAccess";
import StudentDashboard from "./pages/dashboards/StudentDashboard";
import AdminDashboard from "./pages/dashboards/AdminDashboard";

const queryClient = new QueryClient();

const GOOGLE_CLIENT_ID = import.meta.env.VITE_GOOGLE_CLIENT_ID;
const App = () => (
  <GoogleOAuthProvider clientId={GOOGLE_CLIENT_ID}>
    <QueryClientProvider client={queryClient}>
      <TooltipProvider>
        <Toaster />
        <Sonner />
        <BrowserRouter>
          <Routes>
            <Route path="/" element={<Index />} />
            <Route path="/dashboard/student" element={<StudentDashboard />} />
            <Route path="/dashboard/admin" element={<AdminDashboard />} />
            <Route path="/no-access" element={<NoAccess />} />
            <Route path="*" element={<NotFound />} />
          </Routes>
        </BrowserRouter>
      </TooltipProvider>
    </QueryClientProvider>
  </GoogleOAuthProvider>
);

export default App;