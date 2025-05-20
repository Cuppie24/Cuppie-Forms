import { BrowserRouter, Routes, Route } from "react-router-dom";
import EditorPage from "./Pages/EditorPage";
import { ProtectedRoute } from "@/routes/ProtectedRoute";
import AuthPage from "@pages/AuthPage";

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route
          path="/"
          element={
            <ProtectedRoute>
              <EditorPage />
            </ProtectedRoute>
          }
        />
        <Route path="/auth" element={<AuthPage />} />
      </Routes>
    </BrowserRouter>
  );
}
