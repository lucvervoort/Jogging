import axios from 'axios';

const apiClient = axios.create({
	baseURL: 'http://localhost:5187/',
	headers: {
		'Content-Type': 'application/json',
	},
	withCredentials: true,
});

export default apiClient;
