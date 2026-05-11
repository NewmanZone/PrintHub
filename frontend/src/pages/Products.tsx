import React, { useEffect, useState } from 'react';
import { api } from '../services/mockData';
import type { Product } from '../types';

const Products: React.FC = () => {
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');

  useEffect(() => {
    const loadProducts = async () => {
      try {
        const data = await api.getProducts();
        setProducts(data);
      } catch (error) {
        console.error('Failed to load products:', error);
      } finally {
        setLoading(false);
      }
    };
    loadProducts();
  }, []);

  const filteredProducts = products.filter((p) =>
    p.name.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const getStockStatus = (product: Product) => {
    if (product.reorderPoint && product.inventoryOnHand <= product.reorderPoint) {
      return { label: 'Low Stock', className: 'badge-warning' };
    }
    return { label: 'In Stock', className: 'badge-success' };
  };

  if (loading) {
    return (
      <div className="loading">
        <div className="spinner" />
      </div>
    );
  }

  return (
    <div>
      <div className="page-header">
        <h1 className="page-title">Products</h1>
        <p className="page-description">
          Manage your print products and inventory levels.
        </p>
      </div>

      <div className="page-actions">
        <input
          type="text"
          placeholder="Search products..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          className="btn btn-secondary"
          style={{ paddingLeft: '1rem', minWidth: '200px' }}
        />
        <button className="btn btn-primary">+ Add Product</button>
      </div>

      {filteredProducts.length === 0 ? (
        <div className="empty-state">
          <div className="empty-state-icon">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5">
              <path d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
            </svg>
          </div>
          <h3 className="empty-state-title">No products found</h3>
          <p className="empty-state-description">
            {searchTerm ? 'Try adjusting your search terms.' : 'Add your first product to get started.'}
          </p>
        </div>
      ) : (
        <div className="card">
          <div className="table-wrapper">
            <table>
              <thead>
                <tr>
                  <th>Product</th>
                  <th>Price</th>
                  <th>Printed</th>
                  <th>On Hand</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {filteredProducts.map((product) => {
                  const stockStatus = getStockStatus(product);
                  return (
                    <tr key={product.id}>
                      <td>
                        <div className="flex items-center gap-3">
                          {product.imageUrl && (
                            <img
                              src={product.imageUrl}
                              alt={product.name}
                              style={{
                                width: '48px',
                                height: '48px',
                                borderRadius: 'var(--radius-md)',
                                objectFit: 'cover',
                              }}
                            />
                          )}
                          <div>
                            <div style={{ fontWeight: 500 }}>{product.name}</div>
                            <div className="text-sm text-muted">
                              {product.parts?.length || 0} parts
                            </div>
                          </div>
                        </div>
                      </td>
                      <td>${product.etsyPrice?.toFixed(2) || '—'}</td>
                      <td>{product.printCount}</td>
                      <td>
                        <span style={{ fontWeight: product.reorderPoint && product.inventoryOnHand <= product.reorderPoint ? 600 : 400 }}>
                          {product.inventoryOnHand}
                        </span>
                        {product.reorderPoint && (
                          <span className="text-muted text-sm"> / {product.reorderPoint}</span>
                        )}
                      </td>
                      <td>
                        <span className={`badge ${stockStatus.className}`}>
                          {stockStatus.label}
                        </span>
                      </td>
                      <td>
                        <div className="flex gap-2">
                          <button className="btn btn-sm btn-secondary">View</button>
                          <button className="btn btn-sm btn-primary">Print</button>
                        </div>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
};

export default Products;