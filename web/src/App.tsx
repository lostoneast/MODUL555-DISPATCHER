import { Navigate, Outlet, Route, Routes } from "react-router-dom";
import { useSelector } from "react-redux";
import type { RootState } from "./store";
import AppLayout from "./layout/AppLayout";
import LoginPage from "./pages/LoginPage";
import {
  BuildingSectionsPage,
  CarriersPage,
  ConstructionObjectsPage,
  ConstructionTaktsPage,
  FloorsPage,
  LineCapabilitiesPage,
  PlantsPage,
  ProductionLinesPage,
  ProductsPage,
  ProductTypesPage,
  StorageAreasPage,
  TransportRoutesPage,
  UnloadingPointsPage,
  VehiclesPage,
  VehicleTypesPage,
} from "./pages/catalogs";

function RequireAuth() {
  const token = useSelector((s: RootState) => s.auth.token);
  return token ? <Outlet /> : <Navigate to="/login" replace />;
}

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route element={<RequireAuth />}>
        <Route element={<AppLayout />}>
          <Route path="/" element={<ProductsPage />} />
          <Route path="/product-types" element={<ProductTypesPage />} />
          <Route path="/plants" element={<PlantsPage />} />
          <Route path="/production-lines" element={<ProductionLinesPage />} />
          <Route path="/line-capabilities" element={<LineCapabilitiesPage />} />
          <Route
            path="/construction-objects"
            element={<ConstructionObjectsPage />}
          />
          <Route path="/building-sections" element={<BuildingSectionsPage />} />
          <Route path="/floors" element={<FloorsPage />} />
          <Route path="/unloading-points" element={<UnloadingPointsPage />} />
          <Route path="/construction-takts" element={<ConstructionTaktsPage />} />
          <Route path="/storage-areas" element={<StorageAreasPage />} />
          <Route path="/carriers" element={<CarriersPage />} />
          <Route path="/vehicle-types" element={<VehicleTypesPage />} />
          <Route path="/vehicles" element={<VehiclesPage />} />
          <Route path="/transport-routes" element={<TransportRoutesPage />} />
        </Route>
      </Route>
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
