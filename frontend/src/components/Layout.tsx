import { NavLink, Outlet } from 'react-router-dom'

export function Layout() {
  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="brand">
          <span className="brand-mark">F</span>
          <div>
            <strong>Facturas</strong>
            <small>Gestión y reportes</small>
          </div>
        </div>

        <nav className="nav">
          <NavLink
            to="/facturas"
            className={({ isActive }) => (isActive ? 'nav-link active' : 'nav-link')}
          >
            <span className="nav-icon" aria-hidden="true">
              ▤
            </span>
            Facturas
          </NavLink>

          <NavLink
            to="/reportes"
            className={({ isActive }) => (isActive ? 'nav-link active' : 'nav-link')}
          >
            <span className="nav-icon" aria-hidden="true">
              ▦
            </span>
            Reportes
          </NavLink>
        </nav>

        <div className="sidebar-footer">
          <span>API .NET 8 · SQLite</span>
        </div>
      </aside>

      <main className="main">
        <Outlet />
      </main>
    </div>
  )
}
