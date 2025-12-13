import React, { useState } from 'react';
import Card from '../../components/common/Card';
import Button from '../../components/common/Button';
import Input from '../../components/common/Input';
import { MdEmail, MdPhone, MdLocationOn } from 'react-icons/md';
import './Contact.css';

const Contact = () => {
  const [formData, setFormData] = useState({
    name: '',
    email: '',
    subject: '',
    message: ''
  });

  const handleSubmit = (e) => {
    e.preventDefault();
    alert('Thank you for contacting us! We will get back to you soon.');
    setFormData({ name: '', email: '', subject: '', message: '' });
  };

  const handleChange = (e) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value
    });
  };

  return (
    <div className="contact-container">
      <div className="contact-header">
        <h1 className="contact-title">Contact Us</h1>
        <p className="contact-subtitle">We're here to help 24/7</p>
      </div>

      <div className="contact-content">
        <div className="contact-info">
          <Card className="info-card">
            <div className="info-icon"><MdEmail /></div>
            <h3>Email</h3>
            <p>support@silverslayed.com</p>
          </Card>

          <Card className="info-card">
            <div className="info-icon"><MdPhone /></div>
            <h3>Phone</h3>
            <p>1-800-CASINO-1</p>
          </Card>

          <Card className="info-card">
            <div className="info-icon"><MdLocationOn /></div>
            <h3>Address</h3>
            <p>123 Casino Blvd, Las Vegas, NV 89101</p>
          </Card>
        </div>

        <Card className="contact-form-card">
          <h2>Send us a Message</h2>
          <form onSubmit={handleSubmit} className="contact-form">
            <Input
              label="Name"
              name="name"
              value={formData.name}
              onChange={handleChange}
              required
            />
            <Input
              label="Email"
              type="email"
              name="email"
              value={formData.email}
              onChange={handleChange}
              required
            />
            <Input
              label="Subject"
              name="subject"
              value={formData.subject}
              onChange={handleChange}
              required
            />
            <div className="form-group">
              <label htmlFor="message">Message</label>
              <textarea
                id="message"
                name="message"
                value={formData.message}
                onChange={handleChange}
                rows="5"
                required
                className="textarea-input"
              />
            </div>
            <Button type="submit" variant="primary">Send Message</Button>
          </form>
        </Card>
      </div>
    </div>
  );
};

export default Contact;
