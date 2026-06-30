import React from 'react'
import { Navigate, Route, Routes } from 'react-router-dom'
import { AppShell } from './components/AppShell'
import {
  Dashboard,
  Bundles,
  JobDetail,
  Jobs,
  Landing,
  NotFound,
  Orders,
  Parts,
  Printers,
  ProductDetail,
  Products,
  Queue,
  Settings,
} from './pages'

function App() {
  return (
    <Routes>
      <Route path="/" element={<Landing />} />
      <Route
        path="/*"
        element={
          <AppShell>
            <Routes>
              <Route path="/dashboard" element={<Dashboard />} />
              <Route path="/queue" element={<Queue />} />
              <Route path="/orders" element={<Orders />} />
              <Route path="/bundles" element={<Bundles />} />
              <Route path="/products" element={<Products />} />
              <Route path="/products/:id" element={<ProductDetail />} />
              <Route path="/parts" element={<Parts />} />
              <Route path="/jobs" element={<Jobs />} />
              <Route path="/jobs/:id" element={<JobDetail />} />
              <Route path="/printers" element={<Printers />} />
              <Route path="/settings" element={<Settings />} />
              <Route path="/app" element={<Navigate to="/dashboard" replace />} />
              <Route path="*" element={<NotFound />} />
            </Routes>
          </AppShell>
        }
      />
    </Routes>
  )
}

export default App
