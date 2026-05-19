import React from 'react'
import { Routes, Route } from 'react-router-dom'
import { AppShell } from './components/AppShell'
import {
  Dashboard,
  Queue,
  Products,
  ProductDetail,
  Parts,
  Jobs,
  Printers,
  Orders,
  Settings,
} from './pages'

function App() {
  return (
    <AppShell>
      <Routes>
        <Route path="/" element={<Dashboard />} />
        <Route path="/queue" element={<Queue />} />
        <Route path="/products" element={<Products />} />
        <Route path="/products/:id" element={<ProductDetail />} />
        <Route path="/parts" element={<Parts />} />
        <Route path="/jobs" element={<Jobs />} />
        <Route path="/printers" element={<Printers />} />
        <Route path="/orders" element={<Orders />} />
        <Route path="/settings" element={<Settings />} />
      </Routes>
    </AppShell>
  )
}

export default App
