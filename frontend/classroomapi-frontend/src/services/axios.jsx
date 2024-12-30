import axios from 'axios';

const axiosInstance = axios.create({
  baseURL: 'https://localhost:5248/api', // Replace with your .NET API base URL
  headers: {
    'Content-Type': 'application/json',
  },
});

export default axiosInstance;
