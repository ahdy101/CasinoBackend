import React from 'react';
import { NavLink } from 'react-router-dom';
import { MdDashboard, MdPeople, MdGames, MdAttachMoney, MdBarChart, MdSettings } from 'react-icons/md';
import './AdminSidebar.css';

const AdminSidebar = () => {
  const adminSections = [
    {
      id: 'overview',
      name: 'Overview',
      icon: MdDashboard,
      path: '/admin'
    },
    {
      id: 'users',
      name: 'Users',
      icon: MdPeople,
      path: '/admin/users'
    },
    {
      id: 'games',
      name: 'Games',
      icon: MdGames,
      path: '/admin/games'
    },
    {
      id: 'transactions',
      name: 'Transactions',
      icon: MdAttachMoney,
      path: '/admin/transactions'
    },
    {
      id: 'statistics',
      name: 'Statistics',
      icon: MdBarChart,
      path: '/admin/statistics'
    },
    {
      id: 'settings',
      name: 'Settings',
      icon: MdSettings,
      path: '/admin/settings'
    }
  ];

  return (
    <aside className="admin-sidebar">
      <nav className="sidebar-nav">
        {adminSections.map(section => (
          <NavLink
            key={section.id}
            to={section.path}
            end={section.path === '/admin'}
            className={({ isActive }) => 
              `sidebar-link ${isActive ? 'active' : ''}`
            }
          >
            <section.icon className="link-icon" />
            <span className="link-name">{section.name}</span>
          </NavLink>
        ))}
      </nav>
    </aside>
  );
};

export default AdminSidebar;
