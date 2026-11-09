import { Navigate, Route, Routes } from 'react-router-dom'
import { Layout } from './components/Layout'
import { InvoiceDetailPage } from './pages/InvoiceDetailPage'
import { InvoicesPage } from './pages/InvoicesPage'
import { ReportsPage } from './pages/ReportsPage'

export default function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route index element={<Navigate to="/facturas" replace />} />
        <Route path="/facturas" element={<InvoicesPage />} />
        <Route path="/facturas/:invoiceNumber" element={<InvoiceDetailPage />} />
        <Route path="/reportes" element={<ReportsPage />} />
        <Route path="*" element={<Navigate to="/facturas" replace />} />
      </Route>
    </Routes>
  )
}
